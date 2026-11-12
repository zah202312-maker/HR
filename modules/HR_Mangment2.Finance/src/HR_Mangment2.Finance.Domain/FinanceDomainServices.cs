using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Attendance;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Notifications;
using HR_Mangment2.Store;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace HR_Mangment2.Finance;

public static class FinancePeriod
{
    public static string Of(DateOnly date) => date.ToString("yyyy-MM", CultureInfo.InvariantCulture);

    public static DateOnly FirstDay(string period) => DateOnly.ParseExact(period + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static DateOnly LastDay(string period) => FirstDay(period).AddMonths(1).AddDays(-1);

    public static string Next(string period) => Of(FirstDay(period).AddMonths(1));
}

/// <summary>A period is locked once its run is approved or closed. Later financial effects go to the next open period.</summary>
public class PayrollPeriodGuard : ITransientDependency
{
    private readonly IRepository<PayrollRun, Guid> _runs;

    public PayrollPeriodGuard(IRepository<PayrollRun, Guid> runs)
    {
        _runs = runs;
    }

    public virtual Task<bool> IsLockedAsync(string period) =>
        _runs.AnyAsync(x => x.Period == period && x.Status != PayrollRunStatus.Draft);

    /// <summary>The contract keeps a new salary version out of a locked month. A later locked month keeps its fixed run either way.</summary>
    public virtual Task<bool> IsDateLockedAsync(DateOnly effectiveFrom) => IsLockedAsync(FinancePeriod.Of(effectiveFrom));

    public virtual async Task MarkDraftsStaleAsync(string period)
    {
        foreach (var run in await _runs.GetListAsync(x => x.Period == period && x.Status == PayrollRunStatus.Draft))
        {
            run.MarkStale();
            await _runs.UpdateAsync(run, autoSave: true);
        }
    }

    public virtual async Task MarkDraftsStaleFromAsync(DateOnly effectiveFrom)
    {
        var period = FinancePeriod.Of(effectiveFrom);
        var drafts = await _runs.GetListAsync(x => x.Status == PayrollRunStatus.Draft);
        foreach (var run in drafts.Where(x => string.CompareOrdinal(x.Period, period) >= 0))
        {
            run.MarkStale();
            await _runs.UpdateAsync(run, autoSave: true);
        }
    }
}

public class SalaryProfileReader : ITransientDependency
{
    private readonly IRepository<SalaryProfile, Guid> _profiles;
    private readonly IAsyncQueryableExecuter _executer;

    public SalaryProfileReader(IRepository<SalaryProfile, Guid> profiles, IAsyncQueryableExecuter executer)
    {
        _profiles = profiles;
        _executer = executer;
    }

    /// <summary>The version in force on the date, for each employee who has one.</summary>
    public virtual async Task<Dictionary<Guid, SalaryProfile>> GetEffectiveAsync(IReadOnlyCollection<Guid> employeeIds, DateOnly date)
    {
        var query = (await _profiles.WithDetailsAsync(x => x.Components))
            .Where(x => employeeIds.Contains(x.EmployeeId) && x.EffectiveFrom <= date);
        var rows = await _executer.ToListAsync(query);

        return rows.GroupBy(x => x.EmployeeId).ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.EffectiveFrom).First());
    }
}

/// <summary>Builds a run's items from the salary in force on the period's last day and the period's open adjustments.</summary>
public class PayrollCalculator : ITransientDependency
{
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IRepository<Adjustment, Guid> _adjustments;
    private readonly SalaryProfileReader _profiles;
    private readonly IGuidGenerator _guidGenerator;

    public PayrollCalculator(
        IRepository<Employee, Guid> employees,
        IRepository<Adjustment, Guid> adjustments,
        SalaryProfileReader profiles,
        IGuidGenerator guidGenerator)
    {
        _employees = employees;
        _adjustments = adjustments;
        _profiles = profiles;
        _guidGenerator = guidGenerator;
    }

    public virtual async Task CalculateAsync(PayrollRun run, DateTime now)
    {
        var lastDay = FinancePeriod.LastDay(run.Period);
        var employees = await _employees.GetListAsync(x => x.Status != EmployeeStatus.Ended && x.HireDate <= lastDay);
        var employeeIds = employees.Select(x => x.Id).ToList();
        var profiles = await _profiles.GetEffectiveAsync(employeeIds, lastDay);
        var adjustments = (await _adjustments.GetListAsync(x => x.Period == run.Period && x.Status == AdjustmentStatus.Open))
            .ToLookup(x => x.EmployeeId);

        run.Items.Clear();
        foreach (var employee in employees.Where(x => profiles.ContainsKey(x.Id)))
        {
            run.Items.Add(BuildItem(run, profiles[employee.Id], adjustments[employee.Id].OrderBy(x => x.CreationTime).ThenBy(x => x.Id).ToList()));
        }

        var missing = employees.Where(x => !profiles.ContainsKey(x.Id)).Select(x => x.Id.ToString("N"));
        run.MarkCalculated(now, string.Join(',', missing));
    }

    private PayrollItem BuildItem(PayrollRun run, SalaryProfile profile, IReadOnlyCollection<Adjustment> adjustments)
    {
        var allowances = profile.Components.Where(x => x.Type == SalaryComponentType.Allowance).OrderBy(x => x.Name).ToList();
        var fixedDeductions = profile.Components.Where(x => x.Type == SalaryComponentType.FixedDeduction).OrderBy(x => x.Name).ToList();

        var gross = profile.BaseSalary + allowances.Sum(x => x.Amount);
        var additions = adjustments.Where(x => x.Type == AdjustmentType.Addition).Sum(x => x.Amount);
        var deductions = fixedDeductions.Sum(x => x.Amount) + adjustments.Where(x => x.Type == AdjustmentType.Deduction).Sum(x => x.Amount);

        var item = new PayrollItem(_guidGenerator.Create(), run.TenantId, run.Id, profile.EmployeeId, profile.Currency, gross, additions, deductions);

        var order = 0;
        void Add(PayrollLineKind kind, string? label, decimal amount, Guid? adjustmentId) =>
            item.Lines.Add(new PayrollLine(_guidGenerator.Create(), run.TenantId, item.Id, order++, kind, label, amount, adjustmentId));

        Add(PayrollLineKind.BaseSalary, null, profile.BaseSalary, null);
        allowances.ForEach(x => Add(PayrollLineKind.Allowance, x.Name, x.Amount, null));
        fixedDeductions.ForEach(x => Add(PayrollLineKind.FixedDeduction, x.Name, x.Amount, null));
        foreach (var adjustment in adjustments)
        {
            Add(adjustment.Type == AdjustmentType.Addition ? PayrollLineKind.Addition : PayrollLineKind.Deduction, adjustment.Reason, adjustment.Amount, adjustment.Id);
        }

        return item;
    }
}

/// <summary>Generates suggestions from attendance. No amount is suggested, because no formula is approved: see Q119.</summary>
public class AdjustmentSuggestionGenerator : ITransientDependency
{
    private readonly IRepository<AdjustmentSuggestion, Guid> _suggestions;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IRepository<AttendanceCorrection, Guid> _corrections;
    private readonly AttendanceSummaryReader _attendance;
    private readonly SalaryProfileReader _profiles;
    private readonly PayrollPeriodGuard _periods;
    private readonly ICompanyClock _companyClock;
    private readonly IdentityUserManager _userManager;
    private readonly HrNotificationPublisher _notifications;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;

    public AdjustmentSuggestionGenerator(
        IRepository<AdjustmentSuggestion, Guid> suggestions,
        IRepository<Employee, Guid> employees,
        IRepository<AttendanceCorrection, Guid> corrections,
        AttendanceSummaryReader attendance,
        SalaryProfileReader profiles,
        PayrollPeriodGuard periods,
        ICompanyClock companyClock,
        IdentityUserManager userManager,
        HrNotificationPublisher notifications,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant)
    {
        _suggestions = suggestions;
        _employees = employees;
        _corrections = corrections;
        _attendance = attendance;
        _profiles = profiles;
        _periods = periods;
        _companyClock = companyClock;
        _userManager = userManager;
        _notifications = notifications;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
    }

    /// <summary>Safe to repeat. Rows still awaiting review follow the attendance, and reviewed rows never change.</summary>
    public virtual async Task ProcessAsync()
    {
        var today = await _companyClock.GetTodayAsync();
        var created = await GenerateForPeriodAsync(FinancePeriod.Of(today.AddMonths(-1)));
        created += await GenerateSettlementsAsync();

        if (created == 0)
        {
            return;
        }

        foreach (var user in (await _userManager.GetUsersInRoleAsync(HrRoles.Accountant)).Where(x => x.IsActive))
        {
            await _notifications.PublishAsync(user.Id, NotificationTemplateCode.AdjustmentSuggestionsReady, NotificationResourceType.AdjustmentSuggestion, null, created);
        }
    }

    public virtual async Task<int> GenerateForPeriodAsync(string period)
    {
        if (await _periods.IsLockedAsync(period))
        {
            return 0;
        }

        var from = FinancePeriod.FirstDay(period);
        var to = FinancePeriod.LastDay(period);
        var employees = await _employees.GetListAsync(x => x.Status != EmployeeStatus.Ended && x.ManagerEmployeeId != null);
        var employeeIds = employees.Select(x => x.Id).ToList();
        var sums = await _attendance.SumAsync(employeeIds, from, to);
        var profiles = await _profiles.GetEffectiveAsync(employeeIds, to);
        var existing = (await _suggestions.GetListAsync(x => x.Period == period && x.SourceCorrectionId == null))
            .ToDictionary(x => (x.EmployeeId, x.Basis));

        var created = 0;
        foreach (var employeeId in employeeIds)
        {
            var sum = sums.GetValueOrDefault(employeeId, AttendanceSum.Zero);
            var currency = profiles.GetValueOrDefault(employeeId)?.Currency;
            created += await UpsertAsync(existing, employeeId, period, AdjustmentSuggestionBasis.Lateness, sum.LateMinutes, AdjustmentSuggestionQuantityUnit.Minutes, currency);
            created += await UpsertAsync(existing, employeeId, period, AdjustmentSuggestionBasis.Absence, sum.AbsentDays, AdjustmentSuggestionQuantityUnit.Days, currency);
        }

        return created;
    }

    private async Task<int> UpsertAsync(
        Dictionary<(Guid EmployeeId, AdjustmentSuggestionBasis Basis), AdjustmentSuggestion> existing,
        Guid employeeId,
        string period,
        AdjustmentSuggestionBasis basis,
        decimal quantity,
        AdjustmentSuggestionQuantityUnit unit,
        string? currency)
    {
        if (existing.TryGetValue((employeeId, basis), out var row))
        {
            if (row.Status == AdjustmentSuggestionStatus.PendingReview && row.Quantity != quantity)
            {
                row.UpdateQuantity(quantity);
                await _suggestions.UpdateAsync(row);
            }

            return 0;
        }

        if (quantity <= 0)
        {
            return 0;
        }

        await _suggestions.InsertAsync(new AdjustmentSuggestion(_guidGenerator.Create(), _currentTenant.Id, employeeId, period, AdjustmentType.Deduction, basis, quantity, unit, currency));
        return 1;
    }

    /// <summary>A correction applied to a closed month settles in the next period that is still open.</summary>
    public virtual async Task<int> GenerateSettlementsAsync()
    {
        var corrections = await _corrections.GetListAsync(x => x.AffectsClosedMonth == true && x.SettlementPeriod != null);
        if (corrections.Count == 0)
        {
            return 0;
        }

        var ids = corrections.Select(x => x.Id).ToList();
        var done = (await _suggestions.GetListAsync(x => x.SourceCorrectionId != null && ids.Contains(x.SourceCorrectionId.Value)))
            .Select(x => x.SourceCorrectionId!.Value)
            .ToHashSet();

        var created = 0;
        foreach (var correction in corrections.Where(x => !done.Contains(x.Id)))
        {
            var period = correction.SettlementPeriod!;
            while (await _periods.IsLockedAsync(period))
            {
                period = FinancePeriod.Next(period);
            }

            var minutes = correction.ReplacedValue == null || correction.AppliedValue == null
                ? 0m
                : Math.Round((decimal)Math.Abs((correction.AppliedValue.Value - correction.ReplacedValue.Value).TotalMinutes), 2);
            var profiles = await _profiles.GetEffectiveAsync(new[] { correction.EmployeeId }, FinancePeriod.LastDay(period));

            await _suggestions.InsertAsync(new AdjustmentSuggestion(
                _guidGenerator.Create(),
                _currentTenant.Id,
                correction.EmployeeId,
                period,
                AdjustmentType.Addition,
                AdjustmentSuggestionBasis.AttendanceCorrectionSettlement,
                minutes,
                AdjustmentSuggestionQuantityUnit.Minutes,
                profiles.GetValueOrDefault(correction.EmployeeId)?.Currency,
                FinancePeriod.Of(correction.Date),
                correction.Id));
            created++;
        }

        return created;
    }
}

public class AdjustmentSuggestionWorker : AsyncPeriodicBackgroundWorkerBase
{
    public AdjustmentSuggestionWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = (int)TimeSpan.FromHours(6).TotalMilliseconds;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var services = workerContext.ServiceProvider;
        var unitOfWorkManager = services.GetRequiredService<IUnitOfWorkManager>();
        var currentTenant = services.GetRequiredService<ICurrentTenant>();

        List<Guid> tenantIds;
        using (var read = unitOfWorkManager.Begin(requiresNew: true))
        {
            tenantIds = (await services.GetRequiredService<ITenantRepository>().GetListAsync()).Select(x => x.Id).ToList();
            await read.CompleteAsync();
        }

        foreach (var tenantId in tenantIds)
        {
            using (currentTenant.Change(tenantId))
            using (var unitOfWork = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                await services.GetRequiredService<AdjustmentSuggestionGenerator>().ProcessAsync();
                await unitOfWork.CompleteAsync();
            }
        }
    }
}

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IAttendanceClosedMonthReader))]
public class PayrollClosedMonthReader : IAttendanceClosedMonthReader, ITransientDependency
{
    private readonly IRepository<PayrollRun, Guid> _runs;

    public PayrollClosedMonthReader(IRepository<PayrollRun, Guid> runs)
    {
        _runs = runs;
    }

    public Task<bool> IsClosedAsync(DateOnly date)
    {
        var period = FinancePeriod.Of(date);
        return _runs.AnyAsync(x => x.Period == period && x.Status == PayrollRunStatus.Closed);
    }
}

/// <summary>
/// The salary agreed at hiring becomes the employee's first salary version, effective from the hire date and with no
/// components, for the accountant to manage from then on (decision 2026-11-10).
/// </summary>
[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IInitialSalaryGateway))]
public class FinanceInitialSalaryGateway : IInitialSalaryGateway, ITransientDependency
{
    private readonly IRepository<SalaryProfile, Guid> _profiles;
    private readonly PayrollPeriodGuard _periods;
    private readonly IConfiguration _configuration;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;

    public FinanceInitialSalaryGateway(
        IRepository<SalaryProfile, Guid> profiles,
        PayrollPeriodGuard periods,
        IConfiguration configuration,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant)
    {
        _profiles = profiles;
        _periods = periods;
        _configuration = configuration;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
    }

    public virtual async Task RecordAsync(Employee employee, decimal baseSalary, string? currency)
    {
        if (await _periods.IsDateLockedAsync(employee.HireDate))
        {
            throw new Volo.Abp.BusinessException(HR_Mangment2.Errors.HrErrorCodes.Finance.EffectiveDateInLockedPeriod);
        }

        var profile = new SalaryProfile(
            _guidGenerator.Create(),
            _currentTenant.Id,
            employee.Id,
            baseSalary,
            currency ?? _configuration["Finance:DefaultCurrency"] ?? FinanceStockExpenseGateway.DefaultCurrency,
            employee.HireDate);

        await _profiles.InsertAsync(profile, autoSave: true);
        await _periods.MarkDraftsStaleFromAsync(employee.HireDate);
    }
}

/// <summary>The store records a total cost without a currency, so purchases use the configured one: see Q120.</summary>
[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IStockExpenseGateway))]
public class FinanceStockExpenseGateway : IStockExpenseGateway, ITransientDependency
{
    public const string StoreCategory = "store";
    public const string DefaultCurrency = "IQD";

    private readonly IRepository<Expense, Guid> _expenses;
    private readonly ICompanyClock _companyClock;
    private readonly IConfiguration _configuration;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;

    public FinanceStockExpenseGateway(
        IRepository<Expense, Guid> expenses,
        ICompanyClock companyClock,
        IConfiguration configuration,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant)
    {
        _expenses = expenses;
        _companyClock = companyClock;
        _configuration = configuration;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
    }

    public async Task OnPurchaseRecordedAsync(StockMovement movement, string? description = null)
    {
        await _expenses.InsertAsync(new Expense(
            _guidGenerator.Create(),
            _currentTenant.Id,
            StoreCategory,
            movement.TotalCost!.Value,
            _configuration["Finance:DefaultCurrency"] ?? DefaultCurrency,
            await _companyClock.GetTodayAsync(),
            description,
            ExpenseSource.StorePurchase,
            movement.Id), autoSave: true);
    }

    public async Task OnPurchaseReversedAsync(StockMovement original, StockMovement reversal)
    {
        var expense = await _expenses.FirstOrDefaultAsync(x => x.StockMovementId == original.Id);
        if (expense == null || expense.Status == ExpenseStatus.Reversed)
        {
            return;
        }

        expense.MarkReversed();
        await _expenses.UpdateAsync(expense, autoSave: true);

        await _expenses.InsertAsync(new Expense(
            _guidGenerator.Create(),
            _currentTenant.Id,
            expense.Category,
            -expense.Amount,
            expense.Currency,
            await _companyClock.GetTodayAsync(),
            null,
            ExpenseSource.StorePurchase,
            reversal.Id,
            reversalOfExpenseId: expense.Id), autoSave: true);
    }
}
