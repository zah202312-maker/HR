using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Notifications;
using HR_Mangment2.Settings;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace HR_Mangment2.HrEvaluations;

/// <summary>
/// An employee whose average on one heading over one cycle was the threshold or lower (decision 2026-11-11). Read only:
/// the HR manager acts from the other pages. Never shown to the employee.
/// </summary>
public class HrEvaluationAlert : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid CriterionId { get; private set; }
    public decimal AverageScore { get; private set; }
    public int EvaluationsCount { get; private set; }
    public decimal Threshold { get; private set; }
    public DateOnly PeriodFrom { get; private set; }
    public DateOnly PeriodTo { get; private set; }

    protected HrEvaluationAlert()
    {
    }

    public HrEvaluationAlert(Guid id, Guid? tenantId, Guid employeeId, Guid criterionId, decimal averageScore, int evaluationsCount, decimal threshold, DateOnly periodFrom, DateOnly periodTo)
        : base(id)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        CriterionId = criterionId;
        AverageScore = averageScore;
        EvaluationsCount = evaluationsCount;
        Threshold = threshold;
        PeriodFrom = periodFrom;
        PeriodTo = periodTo;
    }
}

/// <summary>
/// Computes the cycle averages of HR evaluations and alerts the HR managers (decision 2026-11-11). Cycles follow each
/// other, so the same scores are never counted twice. The first cycle ends the day before the first check.
/// </summary>
public class HrEvaluationAlertService : ITransientDependency
{
    private readonly IRepository<HrEvaluation, Guid> _evaluations;
    private readonly IRepository<HrEvaluationCriterion, Guid> _criteria;
    private readonly IRepository<HrEvaluationAlert, Guid> _alerts;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IdentityUserManager _userManager;
    private readonly HrSettingsStore _settings;
    private readonly ICompanyClock _companyClock;
    private readonly HrNotificationPublisher _notifications;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    public HrEvaluationAlertService(
        IRepository<HrEvaluation, Guid> evaluations,
        IRepository<HrEvaluationCriterion, Guid> criteria,
        IRepository<HrEvaluationAlert, Guid> alerts,
        IRepository<Employee, Guid> employees,
        IdentityUserManager userManager,
        HrSettingsStore settings,
        ICompanyClock companyClock,
        HrNotificationPublisher notifications,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IAsyncQueryableExecuter asyncExecuter)
    {
        _evaluations = evaluations;
        _criteria = criteria;
        _alerts = alerts;
        _employees = employees;
        _userManager = userManager;
        _settings = settings;
        _companyClock = companyClock;
        _notifications = notifications;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _asyncExecuter = asyncExecuter;
    }

    /// <summary>Computes the cycle that ended yesterday when it is due. Safe to repeat.</summary>
    /// <param name="today">Null means today in the company time zone.</param>
    public virtual async Task CheckDueCycleAsync(DateOnly? today = null)
    {
        var date = today ?? await _companyClock.GetTodayAsync();
        var settings = await _settings.PeekValueAsync<HrEvaluationAlertSettings>();
        var days = settings.WindowWeeks * 7;
        var periodTo = date.AddDays(-1);

        var state = await _settings.GetAsync<HrEvaluationAlertState>();
        if (state.Value.LastPeriodTo != null && periodTo < state.Value.LastPeriodTo.Value.AddDays(days))
        {
            return;
        }

        await ComputeCycleAsync(periodTo.AddDays(-days + 1), periodTo, settings);
        await _settings.SetAsync(new HrEvaluationAlertState { LastPeriodTo = periodTo }, state.ConcurrencyStamp);
    }

    /// <summary>
    /// For each active employee and active heading with at least MinEvaluations evaluations in the period: average =
    /// sum / count. An average at or below the threshold creates one alert and notifies every active HR manager except
    /// the employee themself. Returns the alerts created.
    /// </summary>
    public virtual async Task<List<HrEvaluationAlert>> ComputeCycleAsync(DateOnly periodFrom, DateOnly periodTo, HrEvaluationAlertSettings? settings = null)
    {
        settings ??= await _settings.PeekValueAsync<HrEvaluationAlertSettings>();
        var start = await _companyClock.StartOfDayUtcAsync(periodFrom);
        var end = await _companyClock.StartOfDayUtcAsync(periodTo.AddDays(1));

        var activeCriteria = (await _criteria.GetListAsync(x => x.IsActive)).Select(x => x.Id).ToHashSet();
        var evaluations = await _asyncExecuter.ToListAsync((await _evaluations.WithDetailsAsync(x => x.Scores))
            .Where(x => x.EvaluatedAt >= start && x.EvaluatedAt < end));

        var employeeIds = evaluations.Select(x => x.EmployeeId).Distinct().ToList();
        var employees = (await _employees.GetListAsync(x => employeeIds.Contains(x.Id) && x.Status == EmployeeStatus.Active)).ToDictionary(x => x.Id);
        var existing = (await _alerts.GetListAsync(x => x.PeriodTo == periodTo)).Select(x => (x.EmployeeId, x.CriterionId)).ToHashSet();

        var created = new List<HrEvaluationAlert>();
        foreach (var group in evaluations.Where(x => employees.ContainsKey(x.EmployeeId)).GroupBy(x => x.EmployeeId))
        {
            var scores = group.SelectMany(x => x.Scores).Where(x => activeCriteria.Contains(x.CriterionId)).GroupBy(x => x.CriterionId);
            foreach (var criterion in scores)
            {
                var count = criterion.Count();
                if (count < settings.MinEvaluations || existing.Contains((group.Key, criterion.Key)))
                {
                    continue;
                }

                var average = Math.Round((decimal)criterion.Sum(x => x.Score) / count, 2, MidpointRounding.AwayFromZero);
                if (average > settings.Threshold)
                {
                    continue;
                }

                created.Add(new HrEvaluationAlert(_guidGenerator.Create(), _currentTenant.Id, group.Key, criterion.Key, average, count, settings.Threshold, periodFrom, periodTo));
            }
        }

        if (created.Count == 0)
        {
            return created;
        }

        await _alerts.InsertManyAsync(created, autoSave: true);

        var hrManagers = (await _userManager.GetUsersInRoleAsync(HrRoles.HrManager)).Where(x => x.IsActive).Select(x => x.Id).ToList();
        foreach (var alert in created)
        {
            var employeeUserId = employees[alert.EmployeeId].UserId;
            foreach (var hr in hrManagers.Where(x => x != employeeUserId))
            {
                await _notifications.PublishAsync(hr, NotificationTemplateCode.HrEvaluationLowAverage, NotificationResourceType.HrEvaluationAlert, alert.Id);
            }
        }

        return created;
    }
}

/// <summary>Hourly; a cycle is computed once on its first check after it ends.</summary>
public class HrEvaluationAlertWorker : AsyncPeriodicBackgroundWorkerBase
{
    public HrEvaluationAlertWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = (int)TimeSpan.FromHours(1).TotalMilliseconds;
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
                await services.GetRequiredService<HrEvaluationAlertService>().CheckDueCycleAsync();
                await unitOfWork.CompleteAsync();
            }
        }
    }
}
