using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Common;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Identity;
using HR_Mangment2.Localization;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace HR_Mangment2.Finance;

/// <summary>
/// Petty cash (decision 2026-11-11): types, a fixed monthly amount per employee, and a monthly list the accountant
/// marks as delivered. Delivering creates an expense; undoing it reverses that expense. Never part of the payroll.
/// </summary>
[Authorize]
public class PettyCashAppService : FinanceAppServiceBase, IPettyCashAppService
{
    public const string ExpenseCategory = "pettyCash";
    private const string CreateTypeOperation = "PettyCashType_Create";
    private const string CreateAssignmentOperation = "PettyCashAssignment_Create";

    private readonly IRepository<PettyCashType, Guid> _types;
    private readonly IRepository<PettyCashAssignment, Guid> _assignments;
    private readonly IRepository<PettyCashEntry, Guid> _entries;
    private readonly IRepository<Expense, Guid> _expenses;
    private readonly IdempotencyStore _idempotency;
    private readonly ICompanyClock _companyClock;
    private readonly IConfiguration _configuration;

    public PettyCashAppService(
        IRepository<PettyCashType, Guid> types,
        IRepository<PettyCashAssignment, Guid> assignments,
        IRepository<PettyCashEntry, Guid> entries,
        IRepository<Expense, Guid> expenses,
        IdempotencyStore idempotency,
        ICompanyClock companyClock,
        IConfiguration configuration)
    {
        _types = types;
        _assignments = assignments;
        _entries = entries;
        _expenses = expenses;
        _idempotency = idempotency;
        _companyClock = companyClock;
        _configuration = configuration;
    }

    private string Currency => _configuration["Finance:DefaultCurrency"] ?? FinanceStockExpenseGateway.DefaultCurrency;

    // ------------------------------------------------------------------ types

    [Authorize(HrPermissions.FinancePermissions.PettyCashRead)]
    public virtual async Task<PettyCashTypePagedResultDto> GetTypesAsync(PettyCashTypeListInput input)
    {
        var query = (await _types.GetQueryableAsync()).WhereIf(input.IsActive != null, x => x.IsActive == input.IsActive);
        var total = await AsyncExecuter.LongCountAsync(query);
        var sort = LocalizedName.IsEnglish ? nameof(PettyCashType.NameEn) : nameof(PettyCashType.NameAr);
        var rows = await AsyncExecuter.ToListAsync(query.ApplySorting(input.Sorting, sort, new Dictionary<string, string> { ["name"] = sort }).Skip(input.SkipCount).Take(input.MaxResultCount));
        return new PettyCashTypePagedResultDto { TotalCount = total, Items = rows.Select(ToDto).ToList() };
    }

    [Authorize(HrPermissions.FinancePermissions.PettyCashManage)]
    public virtual async Task<PettyCashTypeDto> CreateTypeAsync(PettyCashTypeCreateInputDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(CreateTypeOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return ToDto(await _types.GetAsync(earlier.Value));
        }

        await CheckTypeNamesAsync(input.NameAr.Trim(), input.NameEn.Trim(), null);
        var type = await _types.InsertAsync(new PettyCashType(GuidGenerator.Create(), CurrentTenant.Id, input.NameAr.Trim(), input.NameEn.Trim()), autoSave: true);
        await _idempotency.SaveAsync(CreateTypeOperation, idempotencyKey, input, type.Id);
        return ToDto(type);
    }

    [Authorize(HrPermissions.FinancePermissions.PettyCashManage)]
    public virtual async Task<PettyCashTypeDto> UpdateTypeAsync(Guid id, PettyCashTypeUpdateInputDto input)
    {
        var type = await _types.GetAsync(id);
        await CheckTypeNamesAsync(input.NameAr.Trim(), input.NameEn.Trim(), id);
        type.ConcurrencyStamp = input.ConcurrencyStamp;
        type.Set(input.NameAr.Trim(), input.NameEn.Trim(), input.IsActive!.Value);
        await _types.UpdateAsync(type, autoSave: true);
        return ToDto(type);
    }

    private async Task CheckTypeNamesAsync(string nameAr, string nameEn, Guid? exceptId)
    {
        if (await _types.AnyAsync(x => (x.NameAr == nameAr || x.NameEn == nameEn) && x.Id != exceptId))
        {
            throw new BusinessException(HrErrorCodes.Finance.PettyCashTypeNameAlreadyExists);
        }
    }

    private static PettyCashTypeDto ToDto(PettyCashType x) => new()
    {
        Id = x.Id, NameAr = x.NameAr, NameEn = x.NameEn, IsActive = x.IsActive, ConcurrencyStamp = x.ConcurrencyStamp, CreationTime = x.CreationTime
    };

    // ------------------------------------------------------------------ assignments

    [Authorize(HrPermissions.FinancePermissions.PettyCashRead)]
    public virtual async Task<PettyCashAssignmentPagedResultDto> GetAssignmentsAsync(PettyCashAssignmentListInput input)
    {
        var query = (await _assignments.GetQueryableAsync())
            .WhereIf(input.EmployeeId != null, x => x.EmployeeId == input.EmployeeId)
            .WhereIf(input.TypeId != null, x => x.TypeId == input.TypeId);

        if (!string.IsNullOrEmpty(input.ActiveIn))
        {
            var p = input.ActiveIn;
            query = query.Where(x => string.Compare(x.FromPeriod, p) <= 0 && (x.ToPeriod == null || string.Compare(x.ToPeriod, p) >= 0));
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount));
        await LogReadAsync(nameof(PettyCashAssignment));
        return new PettyCashAssignmentPagedResultDto { TotalCount = total, Items = await ToDtosAsync(rows) };
    }

    [Authorize(HrPermissions.FinancePermissions.PettyCashManage)]
    public virtual async Task<PettyCashAssignmentDto> CreateAssignmentAsync(PettyCashAssignmentCreateInputDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(CreateAssignmentOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return (await ToDtosAsync(new[] { await _assignments.GetAsync(earlier.Value) })).Single();
        }

        await LoadOtherEmployeeAsync(input.EmployeeId!.Value);
        if (!await _types.AnyAsync(x => x.Id == input.TypeId && x.IsActive))
        {
            throw new BusinessException(HrErrorCodes.Finance.PettyCashTypeInactive);
        }

        CheckRange(input.FromPeriod, input.ToPeriod);
        await CheckOverlapAsync(input.EmployeeId.Value, input.TypeId!.Value, input.FromPeriod, input.ToPeriod, null);

        var assignment = await _assignments.InsertAsync(
            new PettyCashAssignment(GuidGenerator.Create(), CurrentTenant.Id, input.EmployeeId.Value, input.TypeId.Value, input.Amount!.Value, Currency, input.FromPeriod, input.ToPeriod, Clean(input.Note)),
            autoSave: true);
        await _idempotency.SaveAsync(CreateAssignmentOperation, idempotencyKey, input, assignment.Id);
        return (await ToDtosAsync(new[] { assignment })).Single();
    }

    /// <summary>A new amount reaches every month not delivered yet. Stopping cannot cut off a month already delivered.</summary>
    [Authorize(HrPermissions.FinancePermissions.PettyCashManage)]
    public virtual async Task<PettyCashAssignmentDto> UpdateAssignmentAsync(Guid id, PettyCashAssignmentUpdateInputDto input)
    {
        var assignment = await _assignments.GetAsync(id);
        RefuseSelf(await Employees.GetAsync(assignment.EmployeeId));
        CheckRange(assignment.FromPeriod, input.ToPeriod);
        await CheckOverlapAsync(assignment.EmployeeId, assignment.TypeId, assignment.FromPeriod, input.ToPeriod, id);

        var lastDelivered = await AsyncExecuter.FirstOrDefaultAsync((await _entries.GetQueryableAsync())
            .Where(x => x.AssignmentId == id && x.Status == PettyCashEntryStatus.Delivered)
            .OrderByDescending(x => x.Period)
            .Select(x => x.Period));
        if (input.ToPeriod != null && lastDelivered != null && string.CompareOrdinal(input.ToPeriod, lastDelivered) < 0)
        {
            throw new BusinessException(HrErrorCodes.Finance.InvalidPeriodRange);
        }

        assignment.ConcurrencyStamp = input.ConcurrencyStamp;
        assignment.Set(input.Amount!.Value, input.ToPeriod, Clean(input.Note));
        await _assignments.UpdateAsync(assignment, autoSave: true);

        // Pending months follow the new amount; months past the new end that were never delivered go away.
        foreach (var entry in await _entries.GetListAsync(x => x.AssignmentId == id && x.Status == PettyCashEntryStatus.Pending))
        {
            if (!assignment.IsActiveIn(entry.Period))
            {
                await _entries.DeleteAsync(entry, autoSave: true);
            }
            else if (entry.Amount != assignment.Amount)
            {
                entry.ChangeAmount(assignment.Amount);
                await _entries.UpdateAsync(entry, autoSave: true);
            }
        }

        return (await ToDtosAsync(new[] { assignment })).Single();
    }

    private static void CheckRange(string from, string? to)
    {
        if (to != null && string.CompareOrdinal(to, from) < 0)
        {
            throw new BusinessException(HrErrorCodes.Finance.InvalidPeriodRange);
        }
    }

    private async Task CheckOverlapAsync(Guid employeeId, Guid typeId, string from, string? to, Guid? exceptId)
    {
        var others = await _assignments.GetListAsync(x => x.EmployeeId == employeeId && x.TypeId == typeId && x.Id != exceptId);
        var end = to ?? "9999-12";
        if (others.Any(o => string.CompareOrdinal(o.FromPeriod, end) <= 0 && string.CompareOrdinal(o.ToPeriod ?? "9999-12", from) >= 0))
        {
            throw new BusinessException(HrErrorCodes.Finance.PettyCashAssignmentOverlap);
        }
    }

    private async Task<List<PettyCashAssignmentDto>> ToDtosAsync(IReadOnlyCollection<PettyCashAssignment> rows)
    {
        var employees = await References.GetRefsAsync(rows.Select(x => x.EmployeeId));
        var types = await TypeRefsAsync(rows.Select(x => x.TypeId));
        return rows.Select(x => new PettyCashAssignmentDto
        {
            Id = x.Id,
            Employee = employees[x.EmployeeId],
            Type = types[x.TypeId],
            Amount = x.Amount,
            Currency = x.Currency,
            FromPeriod = x.FromPeriod,
            ToPeriod = x.ToPeriod,
            Note = x.Note,
            ConcurrencyStamp = x.ConcurrencyStamp,
            CreationTime = x.CreationTime
        }).ToList();
    }

    // ------------------------------------------------------------------ monthly entries

    /// <summary>Reading a month creates its missing lines from the assignments active in it, so the list is always complete.</summary>
    [Authorize(HrPermissions.FinancePermissions.PettyCashRead)]
    public virtual async Task<PettyCashMonthDto> GetMonthAsync(PettyCashMonthInput input)
    {
        var today = await _companyClock.GetTodayAsync();
        var current = today.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        if (string.CompareOrdinal(input.Period, current) > 0)
        {
            throw new BusinessException(HrErrorCodes.Finance.FuturePeriod);
        }

        await MaterializeAsync(input.Period);

        var all = (await _entries.GetQueryableAsync()).Where(x => x.Period == input.Period);
        var amounts = await AsyncExecuter.ToListAsync(all.Select(x => new { x.Amount, x.Status }));
        var query = all
            .WhereIf(input.Status != null, x => x.Status == input.Status)
            .WhereIf(input.EmployeeId != null, x => x.EmployeeId == input.EmployeeId)
            .WhereIf(input.TypeId != null, x => x.TypeId == input.TypeId);

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.Status).ThenBy(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount));
        await LogReadAsync(nameof(PettyCashEntry));

        return new PettyCashMonthDto
        {
            Period = input.Period,
            Currency = Currency,
            TotalAmount = amounts.Sum(x => x.Amount),
            DeliveredAmount = amounts.Where(x => x.Status == PettyCashEntryStatus.Delivered).Sum(x => x.Amount),
            PendingAmount = amounts.Where(x => x.Status == PettyCashEntryStatus.Pending).Sum(x => x.Amount),
            TotalCount = total,
            Items = await ToEntryDtosAsync(rows)
        };
    }

    private async Task MaterializeAsync(string period)
    {
        var active = await AsyncExecuter.ToListAsync((await _assignments.GetQueryableAsync())
            .Where(x => string.Compare(x.FromPeriod, period) <= 0 && (x.ToPeriod == null || string.Compare(x.ToPeriod, period) >= 0)));
        if (active.Count == 0)
        {
            return;
        }

        var ids = active.Select(x => x.Id).ToList();
        var existing = (await _entries.GetListAsync(x => x.Period == period && ids.Contains(x.AssignmentId))).Select(x => x.AssignmentId).ToHashSet();
        foreach (var assignment in active.Where(x => !existing.Contains(x.Id)))
        {
            await _entries.InsertAsync(new PettyCashEntry(GuidGenerator.Create(), CurrentTenant.Id, assignment, period), autoSave: true);
        }
    }

    [Authorize(HrPermissions.FinancePermissions.PettyCashManage)]
    public virtual async Task<PettyCashEntryDto> UpdateEntryAsync(Guid id, PettyCashEntryUpdateInputDto input)
    {
        var entry = await _entries.GetAsync(id);
        RefuseSelf(await Employees.GetAsync(entry.EmployeeId));
        if (entry.Status != PettyCashEntryStatus.Pending)
        {
            throw new BusinessException(HrErrorCodes.Finance.PettyCashEntryNotPending);
        }

        entry.ConcurrencyStamp = input.ConcurrencyStamp;
        entry.ChangeAmount(input.Amount!.Value);
        await _entries.UpdateAsync(entry, autoSave: true);
        return (await ToEntryDtosAsync(new[] { entry })).Single();
    }

    /// <summary>One transaction: each line becomes delivered with its own expense under the petty cash category.</summary>
    [Authorize(HrPermissions.FinancePermissions.PettyCashManage)]
    public virtual async Task<PettyCashEntryListDto> DeliverAsync(PettyCashDeliveryInputDto input)
    {
        if (input.DeliveredOn > await _companyClock.GetTodayAsync())
        {
            throw new BusinessException(HrErrorCodes.Finance.DeliveryDateInFuture);
        }

        var ids = input.EntryIds.Distinct().ToList();
        var entries = await _entries.GetListAsync(x => ids.Contains(x.Id));
        if (entries.Count != ids.Count)
        {
            throw new EntityNotFoundException(typeof(PettyCashEntry));
        }

        if (entries.Any(x => x.Status != PettyCashEntryStatus.Pending))
        {
            throw new BusinessException(HrErrorCodes.Finance.PettyCashEntryNotPending);
        }

        var employees = (await Employees.GetListAsync(x => entries.Select(e => e.EmployeeId).Contains(x.Id))).ToDictionary(x => x.Id);
        foreach (var entry in entries)
        {
            RefuseSelf(employees[entry.EmployeeId]);
        }

        var types = await TypeRefsAsync(entries.Select(x => x.TypeId));
        var refs = await References.GetRefsAsync(entries.Select(x => x.EmployeeId));
        var me = CurrentUser.GetId();
        foreach (var entry in entries)
        {
            var expense = await _expenses.InsertAsync(
                new Expense(
                    GuidGenerator.Create(),
                    CurrentTenant.Id,
                    ExpenseCategory,
                    entry.Amount,
                    entry.Currency,
                    input.DeliveredOn!.Value,
                    $"{types[entry.TypeId].Name} - {refs[entry.EmployeeId].FullName} - {entry.Period}",
                    ExpenseSource.PettyCash),
                autoSave: true);
            entry.MarkDelivered(input.DeliveredOn.Value, me, expense.Id);
            await _entries.UpdateAsync(entry, autoSave: true);
        }

        return new PettyCashEntryListDto { Items = await ToEntryDtosAsync(entries) };
    }

    /// <summary>Back to pending, and its expense is reversed by a negative one, as financial records never disappear.</summary>
    [Authorize(HrPermissions.FinancePermissions.PettyCashManage)]
    public virtual async Task<PettyCashEntryDto> UndoDeliveryAsync(Guid id, ReasonInputDto input)
    {
        var entry = await _entries.GetAsync(id);
        RefuseSelf(await Employees.GetAsync(entry.EmployeeId));
        if (entry.Status != PettyCashEntryStatus.Delivered)
        {
            throw new BusinessException(HrErrorCodes.Finance.PettyCashEntryNotDelivered);
        }

        var expense = entry.ExpenseId == null ? null : await _expenses.FindAsync(entry.ExpenseId.Value);
        if (expense is { Status: ExpenseStatus.Active })
        {
            expense.MarkReversed();
            await _expenses.UpdateAsync(expense, autoSave: true);
            await _expenses.InsertAsync(
                new Expense(GuidGenerator.Create(), CurrentTenant.Id, expense.Category, -expense.Amount, expense.Currency, await _companyClock.GetTodayAsync(), input.Reason.Trim(), ExpenseSource.PettyCash, reversalOfExpenseId: expense.Id),
                autoSave: true);
        }

        entry.UndoDelivery();
        await _entries.UpdateAsync(entry, autoSave: true);
        return (await ToEntryDtosAsync(new[] { entry })).Single();
    }

    private async Task<List<PettyCashEntryDto>> ToEntryDtosAsync(IReadOnlyCollection<PettyCashEntry> rows)
    {
        var employees = await References.GetRefsAsync(rows.Select(x => x.EmployeeId));
        var types = await TypeRefsAsync(rows.Select(x => x.TypeId));
        var users = await References.GetUserRefsAsync(rows.Where(x => x.DeliveredByUserId != null).Select(x => x.DeliveredByUserId!.Value));
        return rows.Select(x => new PettyCashEntryDto
        {
            Id = x.Id,
            AssignmentId = x.AssignmentId,
            Employee = employees[x.EmployeeId],
            Type = types[x.TypeId],
            Period = x.Period,
            Amount = x.Amount,
            Currency = x.Currency,
            Status = x.Status,
            DeliveredOn = x.DeliveredOn,
            DeliveredBy = x.DeliveredByUserId == null ? null : users.GetValueOrDefault(x.DeliveredByUserId.Value),
            ExpenseId = x.ExpenseId,
            ConcurrencyStamp = x.ConcurrencyStamp
        }).ToList();
    }

    private async Task<Dictionary<Guid, PettyCashTypeRefDto>> TypeRefsAsync(IEnumerable<Guid> ids)
    {
        var distinct = ids.Distinct().ToList();
        return (await _types.GetListAsync(x => distinct.Contains(x.Id)))
            .ToDictionary(x => x.Id, x => new PettyCashTypeRefDto { Id = x.Id, Name = LocalizedName.Pick(x.NameAr, x.NameEn) });
    }

    private static string? Clean(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}

public class PettyCashTypeCreateInputDtoValidator : AbstractValidator<PettyCashTypeCreateInputDto>
{
    public PettyCashTypeCreateInputDtoValidator()
    {
        RuleFor(x => x.NameAr).NotNull().Must(x => !string.IsNullOrWhiteSpace(x)).MaximumLength(128);
        RuleFor(x => x.NameEn).NotNull().Must(x => !string.IsNullOrWhiteSpace(x)).MaximumLength(128);
    }
}

public class PettyCashTypeUpdateInputDtoValidator : AbstractValidator<PettyCashTypeUpdateInputDto>
{
    public PettyCashTypeUpdateInputDtoValidator()
    {
        RuleFor(x => x.NameAr).NotNull().Must(x => !string.IsNullOrWhiteSpace(x)).MaximumLength(128);
        RuleFor(x => x.NameEn).NotNull().Must(x => !string.IsNullOrWhiteSpace(x)).MaximumLength(128);
        RuleFor(x => x.IsActive).NotNull();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public class PettyCashAssignmentCreateInputDtoValidator : AbstractValidator<PettyCashAssignmentCreateInputDto>
{
    public PettyCashAssignmentCreateInputDtoValidator()
    {
        RuleFor(x => x.EmployeeId).NotNull();
        RuleFor(x => x.TypeId).NotNull();
        RuleFor(x => x.Amount).NotNull().GreaterThan(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.FromPeriod).NotNull().Matches(FinanceConsts.PeriodPattern);
        RuleFor(x => x.ToPeriod).Matches(FinanceConsts.PeriodPattern).When(x => x.ToPeriod != null);
        RuleFor(x => x.Note).MaximumLength(FinanceConsts.MaxTextLength);
    }
}

public class PettyCashAssignmentUpdateInputDtoValidator : AbstractValidator<PettyCashAssignmentUpdateInputDto>
{
    public PettyCashAssignmentUpdateInputDtoValidator()
    {
        RuleFor(x => x.Amount).NotNull().GreaterThan(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.ToPeriod).Matches(FinanceConsts.PeriodPattern).When(x => x.ToPeriod != null);
        RuleFor(x => x.Note).MaximumLength(FinanceConsts.MaxTextLength);
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public class PettyCashMonthInputValidator : AbstractValidator<PettyCashMonthInput>
{
    public PettyCashMonthInputValidator()
    {
        RuleFor(x => x.Period).NotNull().Matches(FinanceConsts.PeriodPattern);
    }
}

public class PettyCashEntryUpdateInputDtoValidator : AbstractValidator<PettyCashEntryUpdateInputDto>
{
    public PettyCashEntryUpdateInputDtoValidator()
    {
        RuleFor(x => x.Amount).NotNull().GreaterThan(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public class PettyCashDeliveryInputDtoValidator : AbstractValidator<PettyCashDeliveryInputDto>
{
    public PettyCashDeliveryInputDtoValidator()
    {
        RuleFor(x => x.EntryIds).NotNull().Must(x => x.Count is >= 1 and <= 500);
        RuleFor(x => x.DeliveredOn).NotNull();
    }
}
