using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Common;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Identity;
using HR_Mangment2.Localization;
using HR_Mangment2.Notifications;
using HR_Mangment2.Permissions;
using HR_Mangment2.Requests;
using HR_Mangment2.Settings;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace HR_Mangment2.Leaves;

[Authorize]
public class LeaveBalanceAppService : HR_Mangment2AppService, ILeaveBalanceAppService
{
    private const string AdjustOperation = "LeaveBalance_Adjust";

    /// <summary>The stamp of an employee with no account row yet, so the first entitlement can still be checked like any other.</summary>
    private const string NoAccountStamp = "00000000000000000000000000000000";

    private static readonly Dictionary<string, string> LedgerSortFields = new()
    {
        ["creationTime"] = nameof(LeaveLedgerEntry.CreationTime)
    };

    private readonly IRepository<Employee, Guid> _employees;
    private readonly IRepository<EmployeeLeaveYear, Guid> _years;
    private readonly IRepository<LeaveLedgerEntry, Guid> _ledger;
    private readonly IRepository<OtherLeaveType, Guid> _otherTypes;
    private readonly IRepository<LeaveRequest, Guid> _leaves;
    private readonly IRepository<LeaveRequestSegment, Guid> _segments;
    private readonly IRepository<Request, Guid> _requests;
    private readonly LeaveBalanceManager _balances;
    private readonly DedicatedRequestOwnerResolver _owners;
    private readonly RequestDeciderReader _deciders;
    private readonly ManagerLinkService _managerLinks;
    private readonly EmployeeReferenceReader _references;
    private readonly HrSettingsStore _settings;
    private readonly HrNotificationPublisher _notifications;
    private readonly IdempotencyStore _idempotency;
    private readonly ICompanyClock _companyClock;

    public LeaveBalanceAppService(
        IRepository<Employee, Guid> employees,
        IRepository<EmployeeLeaveYear, Guid> years,
        IRepository<LeaveLedgerEntry, Guid> ledger,
        IRepository<OtherLeaveType, Guid> otherTypes,
        IRepository<LeaveRequest, Guid> leaves,
        IRepository<LeaveRequestSegment, Guid> segments,
        IRepository<Request, Guid> requests,
        LeaveBalanceManager balances,
        DedicatedRequestOwnerResolver owners,
        RequestDeciderReader deciders,
        ManagerLinkService managerLinks,
        EmployeeReferenceReader references,
        HrSettingsStore settings,
        HrNotificationPublisher notifications,
        IdempotencyStore idempotency,
        ICompanyClock companyClock)
    {
        _employees = employees;
        _years = years;
        _ledger = ledger;
        _otherTypes = otherTypes;
        _leaves = leaves;
        _segments = segments;
        _requests = requests;
        _balances = balances;
        _owners = owners;
        _deciders = deciders;
        _managerLinks = managerLinks;
        _references = references;
        _settings = settings;
        _notifications = notifications;
        _idempotency = idempotency;
        _companyClock = companyClock;
    }

    [Authorize(HrPermissions.LeavePermissions.BalanceViewOwn)]
    public virtual async Task<LeaveBalanceDto> GetMyAsync() => await ToDtoAsync(await GetMyEmployeeAsync());

    [Authorize(HrPermissions.LeavePermissions.BalanceView)]
    public virtual async Task<LeaveBalanceDto> GetByEmployeeAsync(Guid employeeId) =>
        await ToDtoAsync(await GetInScopeAsync(employeeId));

    [Authorize(HrPermissions.LeavePermissions.BalanceSetEntitlement)]
    public virtual async Task<LeaveBalanceDto> SetEntitlementAsync(Guid employeeId, LeaveEntitlementUpdateDto input)
    {
        var employee = await GetTargetForChangeAsync(employeeId);
        var snapshot = await _balances.GetSnapshotAsync(employee);

        if ((snapshot.Account?.ConcurrencyStamp ?? NoAccountStamp) != input.ConcurrencyStamp)
        {
            throw new HrConflictException("The balance changed since it was read.");
        }

        var account = await _balances.GetOrCreateAccountAsync(employeeId);
        var days = input.EntitlementDays!.Value;

        if (snapshot.CurrentYear == null)
        {
            await _balances.OpenFirstYearAsync(employee, account, days, input.Reason);
        }
        else
        {
            account.SetNextYearEntitlement(days);
            await _balances.TouchAsync(account);
        }

        await CurrentUnitOfWork!.SaveChangesAsync();
        return await ToDtoAsync(employee);
    }

    [Authorize(HrPermissions.LeavePermissions.BalanceAdjust)]
    public virtual async Task<LeaveBalanceDto> AdjustAsync(Guid employeeId, LeaveBalanceAdjustDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(AdjustOperation, idempotencyKey, new { employeeId, input.Days, input.Reason });
        var employee = await GetTargetForChangeAsync(employeeId);
        if (earlier != null)
        {
            return await ToDtoAsync(employee);
        }

        var snapshot = await _balances.GetSnapshotAsync(employee);
        if (snapshot.CurrentYear == null)
        {
            throw new BusinessException(HrErrorCodes.Leave.EntitlementNotSet);
        }

        var days = input.Days!.Value;
        var spendable = LeaveAllocator.Allocate(snapshot.Pools, Array.Empty<LeaveDemand>()).AvailableBefore;
        if (days < 0 && -days > spendable)
        {
            throw new BusinessException(HrErrorCodes.Leave.AdjustmentExceedsBalance);
        }

        await _balances.AdjustAsync(snapshot, await _balances.GetOrCreateAccountAsync(employeeId), days, input.Reason);

        if (employee.UserId != null)
        {
            await _notifications.PublishAsync(employee.UserId.Value, NotificationTemplateCode.LeaveBalanceAdjusted);
        }

        await _idempotency.SaveAsync(AdjustOperation, idempotencyKey, new { employeeId, input.Days, input.Reason }, employeeId);
        await CurrentUnitOfWork!.SaveChangesAsync();
        return await ToDtoAsync(employee);
    }

    [Authorize(HrPermissions.LeavePermissions.LedgerView)]
    public virtual async Task<LeaveLedgerEntryPagedResultDto> GetLedgerByEmployeeAsync(Guid employeeId, LeaveLedgerListInput input)
    {
        var employee = await GetInScopeAsync(employeeId);
        return await QueryLedgerAsync(employee.Id, input);
    }

    [Authorize(HrPermissions.LeavePermissions.LedgerViewOwn)]
    public virtual async Task<LeaveLedgerEntryPagedResultDto> GetMyLedgerAsync(LeaveLedgerListInput input) =>
        await QueryLedgerAsync((await GetMyEmployeeAsync()).Id, input);

    private async Task<Employee> GetMyEmployeeAsync() =>
        (await _owners.ResolveAsync(CurrentUser.GetId(), null, HrErrorCodes.Leave.ExecutiveDirectorNotEligible, null)).Owner;

    /// <summary>HR sees everyone and a department manager their team. The system administrator and executive see only someone whose request is at their step, and anyone else gets a 404 from them.</summary>
    private async Task<Employee> GetInScopeAsync(Guid employeeId)
    {
        var employee = await _employees.FindAsync(employeeId) ?? throw new EntityNotFoundException(typeof(Employee), employeeId);

        if ((await _managerLinks.GetRolesAsync(employee)).Contains(HrRoles.ExecutiveDirector))
        {
            throw new BusinessException(HrErrorCodes.Leave.ExecutiveDirectorNotEligible);
        }

        var userId = CurrentUser.GetId();
        var roles = (await _deciders.GetAsync(userId)).Roles;
        if (roles.Contains(HrRoles.HrManager))
        {
            return employee;
        }

        if (roles.Contains(HrRoles.DepartmentManager) && await _employees.AnyAsync(x => x.Id == employee.ManagerEmployeeId && x.UserId == userId))
        {
            return employee;
        }

        if (roles.Contains(HrRoles.SystemAdmin) || roles.Contains(HrRoles.ExecutiveDirector))
        {
            var myRoles = roles.ToList();
            var atMyStep = await _requests.AnyAsync(x =>
                x.EmployeeId == employeeId &&
                x.CurrentStepOrder != null &&
                (x.CurrentApproverUserId == userId || (x.CurrentApproverUserId == null && myRoles.Contains(x.CurrentApproverRole!))));

            return atMyStep ? employee : throw new EntityNotFoundException(typeof(Employee), employeeId);
        }

        throw new AbpAuthorizationException();
    }

    /// <summary>Nobody changes their own balance.</summary>
    private async Task<Employee> GetTargetForChangeAsync(Guid employeeId)
    {
        var employee = await _employees.FindAsync(employeeId) ?? throw new EntityNotFoundException(typeof(Employee), employeeId);

        if ((await _managerLinks.GetRolesAsync(employee)).Contains(HrRoles.ExecutiveDirector))
        {
            throw new BusinessException(HrErrorCodes.Leave.ExecutiveDirectorNotEligible);
        }

        if (employee.UserId == CurrentUser.GetId())
        {
            throw new BusinessException(HrErrorCodes.Leave.SelfAdjustmentNotAllowed);
        }

        // D15: the executive director acts on the HR manager's balance only.
        if (!CurrentUser.IsInRole(HrRoles.HrManager) && !(await _managerLinks.GetRolesAsync(employee)).Contains(HrRoles.HrManager))
        {
            throw new Volo.Abp.Authorization.AbpAuthorizationException();
        }

        return employee;
    }

    private async Task<LeaveBalanceDto> ToDtoAsync(Employee employee)
    {
        var snapshot = await _balances.GetSnapshotAsync(employee);
        var year = snapshot.CurrentYear;

        return new LeaveBalanceDto
        {
            Employee = await _references.ToRefAsync(employee),
            IsEntitlementSet = year != null,
            LeaveYearId = year?.Id,
            YearStartDate = snapshot.YearStart,
            YearEndDate = snapshot.YearEnd,
            NextYearStartDate = snapshot.NextYearStart,
            CurrentYearEntitlementDays = year?.EntitlementDays ?? 0,
            CarriedDays = snapshot.Sum(LeaveLedgerType.CarryIn),
            AdjustmentDays = snapshot.Sum(LeaveLedgerType.Adjust),
            UsedDays = -snapshot.Sum(LeaveLedgerType.Deduct, LeaveLedgerType.Reverse),
            HeldDays = -snapshot.Sum(LeaveLedgerType.Hold, LeaveLedgerType.Release),
            RemainingDays = Math.Max(0, snapshot.Sum(LeaveLedgerBucket.Carried) + snapshot.Sum(LeaveLedgerBucket.Entitlement)),
            CarriedRemainingDays = snapshot.Sum(LeaveLedgerBucket.Carried),
            CurrentYearRemainingDays = snapshot.Sum(LeaveLedgerBucket.Entitlement),
            FutureYearHeldDays = -snapshot.FutureEntries.Sum(x => x.Days ?? 0),
            NextYearEntitlementDays = year == null ? null : snapshot.NextYearEntitlement,
            Hourly = await ToHourlyDtoAsync(employee.Id),
            OtherLeaveUsages = await ToOtherUsagesAsync(employee.Id, snapshot.YearStart),
            ConcurrencyStamp = snapshot.Account?.ConcurrencyStamp ?? NoAccountStamp
        };
    }

    private async Task<LeaveHourlyBalanceDto> ToHourlyDtoAsync(Guid employeeId)
    {
        var settings = await _settings.PeekValueAsync<HourlyLeaveSettings>();
        var month = (await _companyClock.GetTodayAsync()).ToString("yyyy-MM");
        var configured = settings.MonthlyAllowedHours > 0 && settings.HoursPerDay > 0;
        var entries = await _balances.GetHourlyMonthEntriesAsync(employeeId, month);
        var quota = configured ? settings.MonthlyAllowedHours!.Value : 0;
        var used = -entries.Where(x => x.Type is LeaveLedgerType.Deduct or LeaveLedgerType.Reverse).Sum(x => x.Hours ?? 0);
        var held = -entries.Where(x => x.Type is LeaveLedgerType.Hold or LeaveLedgerType.Release).Sum(x => x.Hours ?? 0);

        return new LeaveHourlyBalanceDto
        {
            IsConfigured = configured,
            Month = month,
            QuotaHours = quota,
            UsedHours = used,
            HeldHours = held,
            RemainingHours = quota - used - held,
            HoursPerDay = configured ? settings.HoursPerDay : null
        };
    }

    private async Task<List<OtherLeaveUsageDto>> ToOtherUsagesAsync(Guid employeeId, DateOnly yearStart)
    {
        var active = await _otherTypes.GetListAsync(x => x.IsActive);
        var leaves = await _leaves.GetQueryableAsync();
        var query = from segment in await _segments.GetQueryableAsync()
                    join leave in leaves on segment.LeaveRequestId equals leave.Id
                    where leave.EmployeeId == employeeId &&
                          leave.OtherLeaveTypeId != null &&
                          leave.State != LeaveRequestState.Closed &&
                          segment.YearStart == yearStart
                    select new { leave.OtherLeaveTypeId, leave.State, segment.WorkingDays };
        var rows = await AsyncExecuter.ToListAsync(query);

        return active.OrderBy(x => LocalizedName.Pick(x.NameAr, x.NameEn)).Select(type =>
        {
            var used = rows.Where(x => x.OtherLeaveTypeId == type.Id && x.State == LeaveRequestState.Approved).Sum(x => x.WorkingDays);
            var held = rows.Where(x => x.OtherLeaveTypeId == type.Id && x.State == LeaveRequestState.Pending).Sum(x => x.WorkingDays);

            return new OtherLeaveUsageDto
            {
                LeaveTypeId = type.Id,
                LeaveTypeName = LocalizedName.Pick(type.NameAr, type.NameEn),
                UsedDays = used,
                HeldDays = held,
                MaxDays = type.MaxDays,
                RemainingDays = type.MaxDays == null ? null : Math.Max(0, type.MaxDays.Value - used - held)
            };
        }).ToList();
    }

    private async Task<LeaveLedgerEntryPagedResultDto> QueryLedgerAsync(Guid employeeId, LeaveLedgerListInput input)
    {
        var query = (await _ledger.GetQueryableAsync()).Where(x => x.EmployeeId == employeeId);

        if (input.FromDate != null)
        {
            var from = await _companyClock.StartOfDayUtcAsync(input.FromDate.Value);
            query = query.Where(x => x.CreationTime >= from);
        }

        if (input.ToDate != null)
        {
            var to = await _companyClock.StartOfDayUtcAsync(input.ToDate.Value.AddDays(1));
            query = query.Where(x => x.CreationTime < to);
        }

        if (input.LeaveYearId != null)
        {
            query = query.Where(x => x.LeaveYearId == input.LeaveYearId);
        }

        if (input.Type != null)
        {
            query = query.Where(x => x.Type == input.Type);
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(LeaveLedgerEntry.CreationTime) + " desc", LedgerSortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        var yearIds = items.Select(x => x.LeaveYearId).Distinct().ToList();
        var years = (await _years.GetListAsync(x => yearIds.Contains(x.Id))).ToDictionary(x => x.Id);
        var users = await _references.GetUserRefsAsync(items.Where(x => x.CreatorId != null).Select(x => x.CreatorId!.Value));

        return new LeaveLedgerEntryPagedResultDto
        {
            TotalCount = total,
            Items = items.Select(x => new LeaveLedgerEntryDto
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                LeaveYearId = x.LeaveYearId,
                YearStartDate = years[x.LeaveYearId].YearStart,
                YearEndDate = years[x.LeaveYearId].YearEnd,
                Type = x.Type,
                Bucket = x.Bucket,
                Days = x.Days,
                Hours = x.Hours,
                RequestId = x.RequestId,
                Reason = x.Reason,
                CreatedBy = x.CreatorId == null ? null : users.GetValueOrDefault(x.CreatorId.Value),
                CreationTime = x.CreationTime
            }).ToList()
        };
    }
}

public class LeaveEntitlementUpdateDtoValidator : AbstractValidator<LeaveEntitlementUpdateDto>
{
    public LeaveEntitlementUpdateDtoValidator()
    {
        RuleFor(x => x.EntitlementDays).NotNull().GreaterThanOrEqualTo(0);
        RuleFor(x => x.Reason).MaximumLength(LeaveLedgerEntry.MaxReasonLength);
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public class LeaveBalanceAdjustDtoValidator : AbstractValidator<LeaveBalanceAdjustDto>
{
    public LeaveBalanceAdjustDtoValidator()
    {
        RuleFor(x => x.Days).NotNull().NotEqual(0);
        RuleFor(x => x.Reason).NotNull().Length(1, LeaveLedgerEntry.MaxReasonLength);
    }
}
