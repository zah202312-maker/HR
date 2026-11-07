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
using HR_Mangment2.Notifications;
using HR_Mangment2.Permissions;
using HR_Mangment2.Reports;
using HR_Mangment2.Settings;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace HR_Mangment2.Finance;

[Authorize]
public class PayrollRunAppService : FinanceAppServiceBase, IPayrollRunAppService
{
    private const string CreateOperation = "PayrollRun_Create";

    private readonly IRepository<PayrollRun, Guid> _runs;
    private readonly IRepository<PayrollItem, Guid> _items;
    private readonly IRepository<Adjustment, Guid> _adjustments;
    private readonly IRepository<AdjustmentSuggestion, Guid> _suggestions;
    private readonly PayrollCalculator _calculator;
    private readonly HrSettingsStore _settings;
    private readonly IdentityUserManager _userManager;
    private readonly HrNotificationPublisher _notifications;
    private readonly IdempotencyStore _idempotency;

    public PayrollRunAppService(
        IRepository<PayrollRun, Guid> runs,
        IRepository<PayrollItem, Guid> items,
        IRepository<Adjustment, Guid> adjustments,
        IRepository<AdjustmentSuggestion, Guid> suggestions,
        PayrollCalculator calculator,
        HrSettingsStore settings,
        IdentityUserManager userManager,
        HrNotificationPublisher notifications,
        IdempotencyStore idempotency)
    {
        _runs = runs;
        _items = items;
        _adjustments = adjustments;
        _suggestions = suggestions;
        _calculator = calculator;
        _settings = settings;
        _userManager = userManager;
        _notifications = notifications;
        _idempotency = idempotency;
    }

    [Authorize(HrPermissions.FinancePermissions.PayrollRunRead)]
    public virtual async Task<PayrollRunSummaryPagedResultDto> GetListAsync(PayrollRunListInput input)
    {
        await CheckReaderAsync();

        var query = (await WithAllDetailsAsync())
            .WhereIf(input.Status != null, x => x.Status == input.Status)
            .WhereIf(!string.IsNullOrEmpty(input.PeriodFrom), x => string.Compare(x.Period, input.PeriodFrom) >= 0)
            .WhereIf(!string.IsNullOrEmpty(input.PeriodTo), x => string.Compare(x.Period, input.PeriodTo) <= 0);

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.Period).Skip(input.SkipCount).Take(input.MaxResultCount));
        var users = await LoadUsersAsync(rows);

        await LogReadAsync(nameof(PayrollRun));

        return new PayrollRunSummaryPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => Fill(new PayrollRunSummaryDto(), x, users)).ToList()
        };
    }

    [Authorize(HrPermissions.FinancePermissions.PayrollRunPrepare)]
    public virtual async Task<PayrollRunDto> CreateAsync(PayrollRunCreateInputDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(CreateOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return await ToDtoAsync(await LoadAsync(earlier.Value));
        }

        if (await _runs.AnyAsync(x => x.Period == input.Period))
        {
            throw new HrConflictException(L["Conflict:PayrollRunExists"]);
        }

        var run = new PayrollRun(GuidGenerator.Create(), CurrentTenant.Id, input.Period, CurrentUser.GetId(), Clock.Now);
        await _calculator.CalculateAsync(run, Clock.Now);
        await _runs.InsertAsync(run, autoSave: true);
        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, input, run.Id);

        return await ToDtoAsync(run);
    }

    [Authorize(HrPermissions.FinancePermissions.PayrollRunRead)]
    public virtual async Task<PayrollRunDto> GetAsync(Guid id)
    {
        await CheckReaderAsync();
        var run = await LoadAsync(id);
        await LogReadAsync(nameof(PayrollRun), id);
        return await ToDtoAsync(run);
    }

    [Authorize(HrPermissions.FinancePermissions.PayrollRunPrepare)]
    public virtual async Task<PayrollRunDto> RecalculateAsync(Guid id, PayrollRunActionInputDto input)
    {
        var run = await LoadForActionAsync(id, input.ConcurrencyStamp);
        RequireStatus(run, PayrollRunStatus.Draft);

        await _calculator.CalculateAsync(run, Clock.Now);
        await _runs.UpdateAsync(run, autoSave: true);

        return await ToDtoAsync(run);
    }

    [Authorize(HrPermissions.FinancePermissions.PayrollRunPrepare)]
    public virtual async Task<PayrollRunDto> RequestApprovalAsync(Guid id, PayrollRunActionInputDto input)
    {
        var run = await LoadForActionAsync(id, input.ConcurrencyStamp);
        RequireStatus(run, PayrollRunStatus.Draft);

        if (run.IsStale)
        {
            throw new BusinessException(HrErrorCodes.Finance.PayrollRunStale);
        }

        var approverRole = await RequireApproverRoleAsync();
        run.RequestApproval(Clock.Now);
        await _runs.UpdateAsync(run, autoSave: true);

        foreach (var user in (await _userManager.GetUsersInRoleAsync(approverRole)).Where(x => x.IsActive))
        {
            await _notifications.PublishAsync(user.Id, NotificationTemplateCode.PayrollRunPendingApproval, NotificationResourceType.PayrollRun, run.Id);
        }

        return await ToDtoAsync(run);
    }

    [Authorize(HrPermissions.FinancePermissions.PayrollRunApprove)]
    public virtual async Task<PayrollRunDto> ApproveAsync(Guid id, PayrollRunActionInputDto input)
    {
        var run = await LoadForActionAsync(id, input.ConcurrencyStamp);
        RequireStatus(run, PayrollRunStatus.Draft);

        var approverRole = await RequireApproverRoleAsync();
        if (!CurrentUser.IsInRole(approverRole))
        {
            throw new BusinessException(HrErrorCodes.Finance.NotConfiguredPayrollApprover);
        }

        if (run.PreparedByUserId == CurrentUser.GetId())
        {
            throw new BusinessException(HrErrorCodes.Finance.ApproverIsPreparer);
        }

        if (run.ApprovalRequestedAt == null)
        {
            throw new BusinessException(HrErrorCodes.Finance.ApprovalNotRequested);
        }

        if (run.IsStale)
        {
            throw new BusinessException(HrErrorCodes.Finance.PayrollRunStale);
        }

        var adjustmentIds = run.Items.SelectMany(x => x.Lines).Where(x => x.AdjustmentId != null).Select(x => x.AdjustmentId!.Value).ToList();
        foreach (var adjustment in await _adjustments.GetListAsync(x => adjustmentIds.Contains(x.Id)))
        {
            adjustment.MarkIncludedInRun();
            await _adjustments.UpdateAsync(adjustment, autoSave: true);
        }

        run.Approve(CurrentUser.GetId(), Clock.Now);
        await _runs.UpdateAsync(run, autoSave: true);

        foreach (var user in (await _userManager.GetUsersInRoleAsync(HrRoles.Accountant)).Where(x => x.IsActive))
        {
            await _notifications.PublishAsync(user.Id, NotificationTemplateCode.PayrollRunApproved, NotificationResourceType.PayrollRun, run.Id);
        }

        return await ToDtoAsync(run);
    }

    [Authorize(HrPermissions.FinancePermissions.PayrollRunApprove)]
    public virtual async Task<PayrollRunDto> ReturnForChangesAsync(Guid id, ReasonInputDto input)
    {
        await CheckReaderAsync();
        var run = await LoadAsync(id);

        if (run.Status != PayrollRunStatus.Draft || run.ApprovalRequestedAt == null)
        {
            throw new BusinessException(HrErrorCodes.Finance.ApprovalNotRequested);
        }

        run.Return(input.Reason.Trim());
        await _runs.UpdateAsync(run, autoSave: true);

        return await ToDtoAsync(run);
    }

    [Authorize(HrPermissions.FinancePermissions.PayrollRunClose)]
    public virtual async Task<PayrollRunDto> CloseAsync(Guid id, PayrollRunActionInputDto input)
    {
        var run = await LoadForActionAsync(id, input.ConcurrencyStamp);
        RequireStatus(run, PayrollRunStatus.Approved);

        run.Close(Clock.Now);
        await _runs.UpdateAsync(run, autoSave: true);

        var employeeIds = run.Items.Select(x => x.EmployeeId).ToList();
        var employees = (await Employees.GetListAsync(x => employeeIds.Contains(x.Id))).ToDictionary(x => x.Id);
        foreach (var item in run.Items)
        {
            var userId = employees.GetValueOrDefault(item.EmployeeId)?.UserId;
            if (userId != null)
            {
                await _notifications.PublishAsync(userId.Value, NotificationTemplateCode.PayslipAvailable, NotificationResourceType.Payslip, item.Id);
            }
        }

        return await ToDtoAsync(run);
    }

    [Authorize(HrPermissions.FinancePermissions.PayrollRunExport)]
    public virtual async Task<ReportFile> ExportAsync(Guid id, FinanceExportInput input)
    {
        await CheckReaderAsync();
        var run = await LoadAsync(id);
        var dto = await ToDtoAsync(run);

        await LogExportAsync(nameof(PayrollRun), id.ToString());

        return WriteFile(PayrollTables.Statement(L["Report:Title:payrollRun"], ColumnName, run.Period, dto.Items), $"payroll-{run.Period}", input.Format!.Value);
    }

    [Authorize(HrPermissions.FinancePermissions.PayslipReadOwn)]
    public virtual async Task<PayslipSummaryPagedResultDto> GetMyPayslipsAsync(PagedAndSortedResultRequestDto input)
    {
        var me = await Employees.FirstOrDefaultAsync(x => x.UserId == CurrentUser.GetId());
        await LogReadAsync("Payslip");
        if (me == null)
        {
            return new PayslipSummaryPagedResultDto();
        }

        var query =
            from item in await _items.GetQueryableAsync()
            join run in await _runs.GetQueryableAsync() on item.PayrollRunId equals run.Id
            where item.EmployeeId == me.Id && run.Status == PayrollRunStatus.Closed
            orderby run.Period descending
            select new { Item = item, run.Period, run.ClosedAt };

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PayslipSummaryPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => new PayslipSummaryDto
            {
                Id = x.Item.Id,
                Period = x.Period,
                Currency = x.Item.Currency,
                Gross = x.Item.Gross,
                Additions = x.Item.Additions,
                Deductions = x.Item.Deductions,
                Net = x.Item.Net,
                ClosedAt = x.ClosedAt!.Value
            }).ToList()
        };
    }

    [Authorize(HrPermissions.FinancePermissions.PayslipReadOwn)]
    public virtual async Task<PayslipDto> GetMyPayslipAsync(Guid id)
    {
        var payslip = await LoadOwnPayslipAsync(id);
        await LogReadAsync("Payslip", id);
        return payslip;
    }

    [Authorize(HrPermissions.FinancePermissions.PayslipReadOwn)]
    public virtual async Task<ReportFile> DownloadMyPayslipPdfAsync(Guid id)
    {
        var payslip = await LoadOwnPayslipAsync(id);
        await LogReadAsync("Payslip", id);

        var table = new ReportTable(
            L["Report:Title:payslip"],
            $"{payslip.Employee.FullName} - {payslip.Period} ({payslip.Currency})",
            new[] { ColumnName("item"), ColumnName("amount") },
            payslip.Lines.Select(x => new object?[] { x.Label, x.Amount })
                .Append(new object?[] { ColumnName("gross"), payslip.Gross })
                .Append(new object?[] { ColumnName("additions"), payslip.Additions })
                .Append(new object?[] { ColumnName("deductions"), payslip.Deductions })
                .Append(new object?[] { ColumnName("net"), payslip.Net })
                .ToList());

        return WriteFile(table, $"payslip-{payslip.Period}", FinanceExportFormat.Pdf);
    }

    private async Task<PayslipDto> LoadOwnPayslipAsync(Guid id)
    {
        var me = await Employees.FirstOrDefaultAsync(x => x.UserId == CurrentUser.GetId());
        var item = await AsyncExecuter.FirstOrDefaultAsync((await _items.WithDetailsAsync(x => x.Lines)).Where(x => x.Id == id));
        var run = item == null ? null : await _runs.FindAsync(item.PayrollRunId, includeDetails: false);

        if (item == null || run == null || me == null || item.EmployeeId != me.Id || run.Status != PayrollRunStatus.Closed)
        {
            throw new EntityNotFoundException(typeof(PayrollItem), id);
        }

        return new PayslipDto
        {
            Id = item.Id,
            Employee = await References.ToRefAsync(me),
            Period = run.Period,
            Currency = item.Currency,
            Gross = item.Gross,
            Additions = item.Additions,
            Deductions = item.Deductions,
            Net = item.Net,
            Lines = PayrollTables.ToLineDtos(item, L["Finance:BaseSalary"]),
            ClosedAt = run.ClosedAt!.Value
        };
    }

    /// <summary>The accountant always reads. An approver role reads only while the setting names it, otherwise 403.</summary>
    private async Task CheckReaderAsync()
    {
        if (CurrentUser.IsInRole(HrRoles.Accountant))
        {
            return;
        }

        var role = await FindApproverRoleAsync();
        if (role == null || !CurrentUser.IsInRole(role))
        {
            throw new AbpAuthorizationException();
        }
    }

    private async Task<string?> FindApproverRoleAsync()
    {
        var setting = (await _settings.PeekValueAsync<FinanceSettings>()).PayrollApproverRole;
        return setting switch
        {
            PayrollApproverRole.HrManager => HrRoles.HrManager,
            PayrollApproverRole.ExecutiveDirector => HrRoles.ExecutiveDirector,
            _ => null
        };
    }

    private async Task<string> RequireApproverRoleAsync() =>
        await FindApproverRoleAsync() ?? throw new BusinessException(HrErrorCodes.Finance.PayrollApproverNotConfigured);

    private static void RequireStatus(PayrollRun run, PayrollRunStatus status)
    {
        if (run.Status != status)
        {
            throw new BusinessException(HrErrorCodes.Finance.InvalidPayrollRunStatus);
        }
    }

    private async Task<IQueryable<PayrollRun>> WithAllDetailsAsync() =>
        await _runs.WithDetailsAsync();

    private async Task<PayrollRun> LoadAsync(Guid id) =>
        await AsyncExecuter.FirstOrDefaultAsync((await WithAllDetailsAsync()).Where(x => x.Id == id))
        ?? throw new EntityNotFoundException(typeof(PayrollRun), id);

    private async Task<PayrollRun> LoadForActionAsync(Guid id, string concurrencyStamp)
    {
        var run = await LoadAsync(id);
        if (run.ConcurrencyStamp != concurrencyStamp)
        {
            throw new HrConflictException(L["Conflict:RecordChanged"]);
        }

        return run;
    }

    private async Task<Dictionary<Guid, HR_Mangment2.Identity.UserRefDto>> LoadUsersAsync(IReadOnlyCollection<PayrollRun> runs) =>
        await References.GetUserRefsAsync(runs.SelectMany(x => new Guid?[] { x.PreparedByUserId, x.ApprovedByUserId }).Where(x => x != null).Select(x => x!.Value));

    private static T Fill<T>(T dto, PayrollRun run, Dictionary<Guid, HR_Mangment2.Identity.UserRefDto> users) where T : PayrollRunSummaryDto
    {
        dto.Id = run.Id;
        dto.Period = run.Period;
        dto.Status = run.Status;
        dto.PreparedBy = users[run.PreparedByUserId];
        dto.ApprovalRequestedAt = run.ApprovalRequestedAt;
        dto.ApprovedBy = run.ApprovedByUserId == null ? null : users[run.ApprovedByUserId.Value];
        dto.ApprovedAt = run.ApprovedAt;
        dto.ClosedAt = run.ClosedAt;
        dto.LastCalculatedAt = run.LastCalculatedAt;
        dto.IsStale = run.IsStale;
        dto.EmployeeCount = run.Items.Count;
        dto.Totals = PayrollTables.Totals(run.Items);
        dto.ConcurrencyStamp = run.ConcurrencyStamp;
        return dto;
    }

    private async Task<PayrollRunDto> ToDtoAsync(PayrollRun run)
    {
        var users = await LoadUsersAsync(new[] { run });
        var missing = run.EmployeesWithoutProfile.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToList();
        var employees = await References.GetRefsAsync(run.Items.Select(x => x.EmployeeId).Concat(missing));

        var dto = Fill(new PayrollRunDto(), run, users);
        dto.Items = run.Items
            .OrderBy(x => employees[x.EmployeeId].FullName)
            .Select(x => PayrollTables.ToItemDto(x, employees[x.EmployeeId], L["Finance:BaseSalary"]))
            .ToList();
        dto.EmployeesWithoutProfile = missing.Select(x => employees[x]).ToList();
        dto.PendingSuggestionCount = (int)await _suggestions.LongCountAsync(x => x.Period == run.Period && x.Status == AdjustmentSuggestionStatus.PendingReview);
        return dto;
    }
}

/// <summary>The shapes a run's items take, shared by the run, the monthly statement and both exports.</summary>
public static class PayrollTables
{
    public static List<PayrollLineDto> ToLineDtos(PayrollItem item, string baseSalaryLabel) =>
        item.Lines.OrderBy(x => x.Order).Select(x => new PayrollLineDto
        {
            Kind = x.Kind,
            Label = x.Label ?? baseSalaryLabel,
            Amount = x.Amount,
            AdjustmentId = x.AdjustmentId
        }).ToList();

    public static PayrollItemDto ToItemDto(PayrollItem item, EmployeeRefDto employee, string baseSalaryLabel) => new()
    {
        Id = item.Id,
        Employee = employee,
        Currency = item.Currency,
        Gross = item.Gross,
        Additions = item.Additions,
        Deductions = item.Deductions,
        Net = item.Net,
        Lines = ToLineDtos(item, baseSalaryLabel)
    };

    public static List<PayrollTotalDto> Totals(IEnumerable<PayrollItem> items) =>
        items.GroupBy(x => x.Currency).OrderBy(g => g.Key).Select(g => new PayrollTotalDto
        {
            Currency = g.Key,
            Gross = g.Sum(x => x.Gross),
            Additions = g.Sum(x => x.Additions),
            Deductions = g.Sum(x => x.Deductions),
            Net = g.Sum(x => x.Net)
        }).ToList();

    public static ReportTable Statement(string title, Func<string, string> column, string period, IEnumerable<PayrollItemDto> items) => new(
        title,
        period,
        new[] { column("employee"), column("department"), column("currency"), column("gross"), column("additions"), column("deductions"), column("net") },
        items.Select(x => new object?[] { x.Employee.FullName, x.Employee.DepartmentName, x.Currency, x.Gross, x.Additions, x.Deductions, x.Net }).ToList());
}

public class PayrollRunCreateInputDtoValidator : AbstractValidator<PayrollRunCreateInputDto>
{
    public PayrollRunCreateInputDtoValidator()
    {
        RuleFor(x => x.Period).NotNull().Matches(FinanceConsts.PeriodPattern);
    }
}

public class PayrollRunActionInputDtoValidator : AbstractValidator<PayrollRunActionInputDto>
{
    public PayrollRunActionInputDtoValidator()
    {
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
        RuleFor(x => x.Note).MaximumLength(FinanceConsts.MaxTextLength);
    }
}
