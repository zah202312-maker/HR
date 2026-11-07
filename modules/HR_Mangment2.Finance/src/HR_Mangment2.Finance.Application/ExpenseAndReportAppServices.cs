using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Identity;
using HR_Mangment2.Leaves;
using HR_Mangment2.Localization;
using HR_Mangment2.Permissions;
using HR_Mangment2.Reports;
using HR_Mangment2.Requests;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace HR_Mangment2.Finance;

[Authorize]
public class ExpenseAppService : FinanceAppServiceBase, IExpenseAppService
{
    private const string CreateOperation = "Expense_Create";

    private static readonly Dictionary<string, string> Sort = new()
    {
        ["date"] = nameof(Expense.Date),
        ["amount"] = nameof(Expense.Amount),
        ["category"] = nameof(Expense.Category)
    };

    private readonly IRepository<Expense, Guid> _expenses;
    private readonly ICompanyClock _companyClock;
    private readonly IdempotencyStore _idempotency;

    public ExpenseAppService(IRepository<Expense, Guid> expenses, ICompanyClock companyClock, IdempotencyStore idempotency)
    {
        _expenses = expenses;
        _companyClock = companyClock;
        _idempotency = idempotency;
    }

    [Authorize(HrPermissions.FinancePermissions.ExpenseRead)]
    public virtual async Task<ExpensePagedResultDto> GetListAsync(ExpenseListInput input)
    {
        var query = Filter(await _expenses.GetQueryableAsync(), input.Source, input.Category, input.DateFrom, input.DateTo)
            .WhereIf(input.Status != null, x => x.Status == input.Status);

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query
            .ApplySorting(input.Sorting, nameof(Expense.Date) + " desc", Sort)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount));

        await LogReadAsync(nameof(Expense));
        return new ExpensePagedResultDto { TotalCount = total, Items = await ExpenseMapper.ToDtosAsync(References, rows) };
    }

    [Authorize(HrPermissions.FinancePermissions.ExpenseCreate)]
    public virtual async Task<ExpenseDto> CreateAsync(ExpenseCreateInput input, Guid? idempotencyKey)
    {
        var fingerprint = new { input.Category, input.Amount, input.Currency, input.Date, input.Description, input.SourceRequestId, input.Attachment.FileName, input.Attachment.Content.Length };
        var earlier = await _idempotency.FindResourceAsync(CreateOperation, idempotencyKey, fingerprint);
        if (earlier != null)
        {
            return await ToDtoAsync(await _expenses.GetAsync(earlier.Value));
        }

        await CheckRequestAtAccountantStepAsync(input.SourceRequestId);

        var file = await StoreAttachmentAsync(input.Attachment);
        var expense = new Expense(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            input.Category.Trim(),
            input.Amount,
            input.Currency,
            input.Date!.Value,
            input.Description,
            ExpenseSource.General,
            sourceRequestId: input.SourceRequestId);
        expense.AttachFile(file.Id, file.FileName, input.Attachment.ContentType, input.Attachment.Content.LongLength, file.BlobName);

        await _expenses.InsertAsync(expense, autoSave: true);
        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, fingerprint, expense.Id);

        return await ToDtoAsync(expense);
    }

    [Authorize(HrPermissions.FinancePermissions.ExpenseRead)]
    public virtual async Task<ReportFile> ExportAsync(ExpenseExportInput input)
    {
        var rows = await AsyncExecuter.ToListAsync(Filter(await _expenses.GetQueryableAsync(), input.Source, input.Category, input.DateFrom, input.DateTo)
            .OrderByDescending(x => x.Date)
            .ThenBy(x => x.Id));

        await LogExportAsync(nameof(Expense), null);

        return WriteFile(ExpenseMapper.Table(L["Report:Title:expenses"], ColumnName, rows), "expenses", input.Format!.Value);
    }

    [Authorize(HrPermissions.FinancePermissions.ExpenseRead)]
    public virtual async Task<ExpenseDto> GetAsync(Guid id)
    {
        var expense = await _expenses.GetAsync(id);
        await LogReadAsync(nameof(Expense), id);
        return await ToDtoAsync(expense);
    }

    [Authorize(HrPermissions.FinancePermissions.ExpenseCreate)]
    public virtual async Task<ExpenseDto> ReverseAsync(Guid id, ReasonInputDto input)
    {
        var original = await _expenses.GetAsync(id);
        if (original.Status == ExpenseStatus.Reversed)
        {
            throw new BusinessException(HrErrorCodes.Finance.ExpenseAlreadyReversed);
        }

        original.MarkReversed();
        await _expenses.UpdateAsync(original, autoSave: true);

        var reversal = new Expense(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            original.Category,
            -original.Amount,
            original.Currency,
            await _companyClock.GetTodayAsync(),
            input.Reason.Trim(),
            original.Source,
            original.StockMovementId,
            original.SourceRequestId,
            original.Id);
        await _expenses.InsertAsync(reversal, autoSave: true);

        return await ToDtoAsync(reversal);
    }

    [Authorize(HrPermissions.FinancePermissions.ExpenseRead)]
    public virtual async Task<FileDownloadUrlDto> GetAttachmentDownloadUrlAsync(Guid id)
    {
        var expense = await _expenses.GetAsync(id);
        if (expense.AttachmentId == null)
        {
            throw new EntityNotFoundException(typeof(Expense), id);
        }

        await LogReadAsync(nameof(Expense), id);
        var link = await Links.IssueAsync(expense.AttachmentId.Value, expense.AttachmentBlobName!, expense.AttachmentFileName!, expense.AttachmentContentType!);
        return new FileDownloadUrlDto { Url = link.Url, ExpiresAt = link.ExpiresAt };
    }

    private static IQueryable<Expense> Filter(IQueryable<Expense> query, ExpenseSource? source, string? category, DateOnly? from, DateOnly? to) =>
        query
            .WhereIf(source != null, x => x.Source == source)
            .WhereIf(!string.IsNullOrWhiteSpace(category), x => x.Category == category)
            .WhereIf(from != null, x => x.Date >= from)
            .WhereIf(to != null, x => x.Date <= to);

    private async Task<ExpenseDto> ToDtoAsync(Expense expense) => (await ExpenseMapper.ToDtosAsync(References, new[] { expense }))[0];
}

public static class ExpenseMapper
{
    public static async Task<List<ExpenseDto>> ToDtosAsync(EmployeeReferenceReader references, IReadOnlyCollection<Expense> rows)
    {
        var users = await references.GetUserRefsAsync(rows.Where(x => x.CreatorId != null).Select(x => x.CreatorId!.Value));

        return rows.Select(x => new ExpenseDto
        {
            Id = x.Id,
            Category = x.Category,
            Amount = x.Amount,
            Currency = x.Currency,
            Date = x.Date,
            Description = x.Description,
            Source = x.Source,
            Status = x.Status,
            Attachment = x.AttachmentId == null ? null : new AttachmentDto
            {
                Id = x.AttachmentId.Value,
                FileName = x.AttachmentFileName!,
                ContentType = x.AttachmentContentType!,
                SizeBytes = x.AttachmentSizeBytes!.Value,
                CreationTime = x.CreationTime
            },
            StockMovementId = x.StockMovementId,
            SourceRequestId = x.SourceRequestId,
            ReversalOfExpenseId = x.ReversalOfExpenseId,
            CreatedBy = x.CreatorId == null ? null : users.GetValueOrDefault(x.CreatorId.Value),
            CreationTime = x.CreationTime
        }).ToList();
    }

    public static ReportTable Table(string title, Func<string, string> column, IEnumerable<Expense> rows) => new(
        title,
        null,
        new[] { column("date"), column("category"), column("amount"), column("currency"), column("description") },
        rows.Select(x => new object?[] { x.Date.ToString("yyyy-MM-dd"), x.Category, x.Amount, x.Currency, x.Description }).ToList());
}

[Authorize]
public class AccountantRequestAppService : FinanceAppServiceBase, IAccountantRequestAppService
{
    private static readonly RequestStatus[] VisibleStatuses = { RequestStatus.PendingDestination, RequestStatus.Completed, RequestStatus.Rejected };

    private readonly IRepository<Request, Guid> _requests;
    private readonly IRepository<RequestType, Guid> _types;
    private readonly IRepository<Adjustment, Guid> _adjustments;
    private readonly IRepository<Expense, Guid> _expenses;

    public AccountantRequestAppService(
        IRepository<Request, Guid> requests,
        IRepository<RequestType, Guid> types,
        IRepository<Adjustment, Guid> adjustments,
        IRepository<Expense, Guid> expenses)
    {
        _requests = requests;
        _types = types;
        _adjustments = adjustments;
        _expenses = expenses;
    }

    [Authorize(HrPermissions.FinancePermissions.AccountantRequestRead)]
    public virtual async Task<AccountantRequestSummaryPagedResultDto> GetListAsync(AccountantRequestListInput input)
    {
        var query = (await ReachedAsync())
            .WhereIf(input.EmployeeId != null, x => x.EmployeeId == input.EmployeeId)
            .WhereIf(input.Status == AccountantRequestStatus.PendingAccountant, x => x.Status == RequestStatus.PendingDestination)
            .WhereIf(input.Status == AccountantRequestStatus.Completed, x => x.Status == RequestStatus.Completed)
            .WhereIf(input.Status == AccountantRequestStatus.Rejected, x => x.Status == RequestStatus.Rejected);

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.CurrentStepSince).ThenBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount));
        var employees = await References.GetRefsAsync(rows.Select(x => x.EmployeeId));
        var types = await LoadTypesAsync(rows);

        return new AccountantRequestSummaryPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => new AccountantRequestSummaryDto
            {
                Id = x.Id,
                Employee = employees[x.EmployeeId],
                RequestTypeCode = types[x.RequestTypeId].Code,
                RequestTypeName = LocalizedName.Pick(types[x.RequestTypeId].NameAr, types[x.RequestTypeId].NameEn),
                Status = ToStatus(x.Status),
                ArrivedAt = ArrivedAt(x),
                DecidedAt = AccountantStep(x).DecidedAt
            }).ToList()
        };
    }

    [Authorize(HrPermissions.FinancePermissions.AccountantRequestRead)]
    public virtual async Task<AccountantRequestDto> GetAsync(Guid id)
    {
        var request = await AsyncExecuter.FirstOrDefaultAsync((await ReachedAsync()).Where(x => x.Id == id))
                      ?? throw new EntityNotFoundException(typeof(Request), id);

        var employees = await References.GetRefsAsync(new[] { request.EmployeeId });
        var types = await LoadTypesAsync(new[] { request });
        var timeline = request.Timeline.OrderBy(x => x.OccurredAt).ThenBy(x => x.Id).ToList();
        var users = await References.GetUserRefsAsync(timeline.Where(x => x.ActorUserId != null).Select(x => x.ActorUserId!.Value).Append(request.SubmittedByUserId));

        return new AccountantRequestDto
        {
            Id = request.Id,
            Employee = employees[request.EmployeeId],
            SubmittedBy = users[request.SubmittedByUserId],
            RequestTypeCode = types[request.RequestTypeId].Code,
            RequestTypeName = LocalizedName.Pick(types[request.RequestTypeId].NameAr, types[request.RequestTypeId].NameEn),
            Description = request.Description,
            Status = ToStatus(request.Status),
            ArrivedAt = ArrivedAt(request),
            DecidedAt = AccountantStep(request).DecidedAt,
            Timeline = timeline.Select(x => new TimelineEntryDto
            {
                OccurredAt = x.OccurredAt,
                Action = x.Action,
                Actor = x.ActorUserId == null ? null : users[x.ActorUserId.Value],
                Note = x.Note
            }).ToList(),
            LinkedAdjustmentIds = (await _adjustments.GetListAsync(x => x.SourceRequestId == id)).Select(x => x.Id).ToList(),
            LinkedExpenseIds = (await _expenses.GetListAsync(x => x.SourceRequestId == id)).Select(x => x.Id).ToList()
        };
    }

    /// <summary>Only requests that reached the accountant's step. One rejected on an earlier step never arrived.</summary>
    private async Task<IQueryable<Request>> ReachedAsync() =>
        (await _requests.WithDetailsAsync(x => x.Steps, x => x.Timeline))
        .Where(x => x.Destination == RequestDestination.Accountant && VisibleStatuses.Contains(x.Status))
        .Where(x => x.Steps.Any(s => s.ApproverRole == HrRoles.Accountant && s.Decision != RequestStepDecision.NotReached));

    private static RequestStep AccountantStep(Request request) => request.Steps.First(x => x.ApproverRole == HrRoles.Accountant);

    private static DateTime ArrivedAt(Request request)
    {
        var step = AccountantStep(request);
        var previous = request.Steps.Where(x => x.Order < step.Order).OrderBy(x => x.Order).LastOrDefault();
        return previous?.DecidedAt ?? request.CreationTime;
    }

    private static AccountantRequestStatus ToStatus(RequestStatus status) => status switch
    {
        RequestStatus.Completed => AccountantRequestStatus.Completed,
        RequestStatus.Rejected => AccountantRequestStatus.Rejected,
        _ => AccountantRequestStatus.PendingAccountant
    };

    private async Task<Dictionary<Guid, RequestType>> LoadTypesAsync(IReadOnlyCollection<Request> requests)
    {
        var ids = requests.Select(x => x.RequestTypeId).Distinct().ToList();
        return (await _types.GetListAsync(x => ids.Contains(x.Id))).ToDictionary(x => x.Id);
    }
}

[Authorize]
public class FinanceReportAppService : FinanceAppServiceBase, IFinanceReportAppService
{
    private readonly IRepository<PayrollRun, Guid> _runs;
    private readonly IRepository<Adjustment, Guid> _adjustments;
    private readonly IRepository<Expense, Guid> _expenses;

    public FinanceReportAppService(IRepository<PayrollRun, Guid> runs, IRepository<Adjustment, Guid> adjustments, IRepository<Expense, Guid> expenses)
    {
        _runs = runs;
        _adjustments = adjustments;
        _expenses = expenses;
    }

    [Authorize(HrPermissions.FinancePermissions.ReportRead)]
    public virtual async Task<FinanceMonthlyStatementReportDto> GetMonthlyStatementAsync(FinanceMonthlyStatementInput input)
    {
        var report = await BuildStatementAsync(input.Period, input.DepartmentId);
        await LogReadAsync("FinanceReport.monthlyStatement");
        return report;
    }

    [Authorize(HrPermissions.FinancePermissions.ReportRead)]
    public virtual async Task<FinanceAdjustmentReportPagedResultDto> GetAdjustmentsAsync(FinanceAdjustmentReportInput input)
    {
        var query = await AdjustmentQueryAsync(input.PeriodFrom, input.PeriodTo, input.EmployeeId, input.DepartmentId, input.Type);
        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.Period).ThenBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount));
        var totals = await AsyncExecuter.ToListAsync(query.Select(x => new { x.Currency, x.Type, x.Amount }));

        await LogReadAsync("FinanceReport.adjustments");

        return new FinanceAdjustmentReportPagedResultDto
        {
            TotalCount = total,
            Items = await AdjustmentMapper.ToDtosAsync(References, rows),
            Totals = totals.GroupBy(x => x.Currency).OrderBy(g => g.Key).Select(g => new FinanceAdjustmentTotalDto
            {
                Currency = g.Key,
                Additions = g.Where(x => x.Type == AdjustmentType.Addition).Sum(x => x.Amount),
                Deductions = g.Where(x => x.Type == AdjustmentType.Deduction).Sum(x => x.Amount)
            }).ToList()
        };
    }

    [Authorize(HrPermissions.FinancePermissions.ReportRead)]
    public virtual async Task<FinanceStoreExpenseReportPagedResultDto> GetStoreExpensesAsync(FinanceStoreExpenseReportInput input)
    {
        var query = await StoreExpenseQueryAsync(input.DateFrom!.Value, input.DateTo!.Value);
        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.Date).ThenBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount));
        var totals = await AsyncExecuter.ToListAsync(query.Select(x => new { x.Currency, x.Amount }));

        await LogReadAsync("FinanceReport.storeExpenses");

        return new FinanceStoreExpenseReportPagedResultDto
        {
            TotalCount = total,
            Items = await ExpenseMapper.ToDtosAsync(References, rows),
            Totals = totals.GroupBy(x => x.Currency).OrderBy(g => g.Key).Select(g => new FinanceAmountTotalDto { Currency = g.Key, Amount = g.Sum(x => x.Amount) }).ToList()
        };
    }

    [Authorize(HrPermissions.FinancePermissions.ReportRead)]
    public virtual async Task<ReportFile> ExportAsync(FinanceReportCode code, FinanceReportExportInput input)
    {
        var format = input.Format!.Value;
        var name = EnumName(code);
        var title = L["Report:Title:" + name];
        ReportFile file;

        switch (code)
        {
            case FinanceReportCode.MonthlyStatement:
            {
                if (string.IsNullOrEmpty(input.Period))
                {
                    throw new BusinessException(HrErrorCodes.Finance.ReportFilterRequired);
                }

                var report = await BuildStatementAsync(input.Period, input.DepartmentId);
                file = WriteFile(PayrollTables.Statement(title, ColumnName, input.Period, report.Items), name, format);
                break;
            }

            case FinanceReportCode.Adjustments:
            {
                if (string.IsNullOrEmpty(input.PeriodFrom) || string.IsNullOrEmpty(input.PeriodTo))
                {
                    throw new BusinessException(HrErrorCodes.Finance.ReportFilterRequired);
                }

                var rows = await AsyncExecuter.ToListAsync((await AdjustmentQueryAsync(input.PeriodFrom, input.PeriodTo, input.EmployeeId, input.DepartmentId, input.Type))
                    .OrderByDescending(x => x.Period)
                    .ThenBy(x => x.Id));
                var dtos = await AdjustmentMapper.ToDtosAsync(References, rows);
                var table = new ReportTable(
                    title,
                    $"{input.PeriodFrom} - {input.PeriodTo}",
                    new[] { ColumnName("employee"), ColumnName("department"), ColumnName("period"), ColumnName("type"), ColumnName("amount"), ColumnName("currency"), ColumnName("reason") },
                    dtos.Select(x => new object?[] { x.Employee.FullName, x.Employee.DepartmentName, x.Period, L["Finance:AdjustmentType:" + EnumName(x.Type)].Value, x.Amount, x.Currency, x.Reason }).ToList());
                file = WriteFile(table, name, format);
                break;
            }

            default:
            {
                if (input.DateFrom == null || input.DateTo == null)
                {
                    throw new BusinessException(HrErrorCodes.Finance.ReportFilterRequired);
                }

                var rows = await AsyncExecuter.ToListAsync((await StoreExpenseQueryAsync(input.DateFrom.Value, input.DateTo.Value)).OrderByDescending(x => x.Date).ThenBy(x => x.Id));
                file = WriteFile(ExpenseMapper.Table(title, ColumnName, rows), name, format);
                break;
            }
        }

        await LogExportAsync("FinanceReport", name);
        return file;
    }

    private async Task<FinanceMonthlyStatementReportDto> BuildStatementAsync(string period, Guid? departmentId)
    {
        var run = await AsyncExecuter.FirstOrDefaultAsync((await _runs.WithDetailsAsync()).Where(x => x.Period == period))
                  ?? throw new BusinessException(HrErrorCodes.Finance.PayrollRunNotFoundForPeriod);

        var departmentIds = await DepartmentEmployeeIdsAsync(departmentId);
        var items = run.Items.Where(x => departmentIds == null || departmentIds.Contains(x.EmployeeId)).ToList();
        var employees = await References.GetRefsAsync(items.Select(x => x.EmployeeId));

        return new FinanceMonthlyStatementReportDto
        {
            Period = period,
            RunId = run.Id,
            RunStatus = run.Status,
            Items = items.OrderBy(x => employees[x.EmployeeId].FullName).Select(x => PayrollTables.ToItemDto(x, employees[x.EmployeeId], L["Finance:BaseSalary"])).ToList(),
            Totals = PayrollTables.Totals(items)
        };
    }

    private async Task<IQueryable<Adjustment>> AdjustmentQueryAsync(string from, string to, Guid? employeeId, Guid? departmentId, AdjustmentType? type)
    {
        var departmentIds = await DepartmentEmployeeIdsAsync(departmentId);
        return (await _adjustments.GetQueryableAsync())
            .Where(x => string.Compare(x.Period, from) >= 0 && string.Compare(x.Period, to) <= 0)
            .WhereIf(employeeId != null, x => x.EmployeeId == employeeId)
            .WhereIf(departmentIds != null, x => departmentIds!.Contains(x.EmployeeId))
            .WhereIf(type != null, x => x.Type == type);
    }

    private async Task<IQueryable<Expense>> StoreExpenseQueryAsync(DateOnly from, DateOnly to) =>
        (await _expenses.GetQueryableAsync()).Where(x => x.Source == ExpenseSource.StorePurchase && x.Date >= from && x.Date <= to);
}

public class ExpenseCreateInputValidator : AbstractValidator<ExpenseCreateInput>
{
    public ExpenseCreateInputValidator()
    {
        RuleFor(x => x.Category).NotNull().Length(1, FinanceConsts.MaxCategoryLength);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0.01m);
        RuleFor(x => x.Currency).NotNull().Matches(FinanceConsts.CurrencyPattern);
        RuleFor(x => x.Date).NotNull();
        RuleFor(x => x.Description).MaximumLength(FinanceConsts.MaxTextLength);
        RuleFor(x => x.Attachment).NotNull();
    }
}

public class FinanceExportInputValidator : AbstractValidator<FinanceExportInput>
{
    public FinanceExportInputValidator()
    {
        RuleFor(x => x.Format).NotNull();
    }
}

public class ExpenseExportInputValidator : AbstractValidator<ExpenseExportInput>
{
    public ExpenseExportInputValidator()
    {
        RuleFor(x => x.Format).NotNull();
    }
}

public class FinanceReportExportInputValidator : AbstractValidator<FinanceReportExportInput>
{
    public FinanceReportExportInputValidator()
    {
        RuleFor(x => x.Format).NotNull();
        RuleFor(x => x.Period).Matches(FinanceConsts.PeriodPattern).When(x => x.Period != null);
        RuleFor(x => x.PeriodFrom).Matches(FinanceConsts.PeriodPattern).When(x => x.PeriodFrom != null);
        RuleFor(x => x.PeriodTo).Matches(FinanceConsts.PeriodPattern).When(x => x.PeriodTo != null);
    }
}

public class FinanceMonthlyStatementInputValidator : AbstractValidator<FinanceMonthlyStatementInput>
{
    public FinanceMonthlyStatementInputValidator()
    {
        RuleFor(x => x.Period).NotNull().Matches(FinanceConsts.PeriodPattern);
    }
}

public class FinanceAdjustmentReportInputValidator : AbstractValidator<FinanceAdjustmentReportInput>
{
    public FinanceAdjustmentReportInputValidator()
    {
        RuleFor(x => x.PeriodFrom).NotNull().Matches(FinanceConsts.PeriodPattern);
        RuleFor(x => x.PeriodTo).NotNull().Matches(FinanceConsts.PeriodPattern);
    }
}

public class FinanceStoreExpenseReportInputValidator : AbstractValidator<FinanceStoreExpenseReportInput>
{
    public FinanceStoreExpenseReportInputValidator()
    {
        RuleFor(x => x.DateFrom).NotNull();
        RuleFor(x => x.DateTo).NotNull();
    }
}
