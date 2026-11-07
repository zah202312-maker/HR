using System;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Notifications;
using HR_Mangment2.Settings;
using Volo.Abp.Auditing;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.EmploymentContracts;

/// <summary>The department manager has no part in this flow, and nothing reaches the employee before the decision.</summary>
[Audited]
public class EmploymentContract : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxContractTypeLength = 100;
    public const int MaxTextLength = 2000;

    public Guid? TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public string ContractType { get; private set; } = null!;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public Guid? PreviousContractId { get; private set; }
    public ContractStatus Status { get; private set; }

    public string? EmployeePosition { get; private set; }
    public ContractRenewalOption? Recommendation { get; private set; }
    public string? RecommendationNote { get; private set; }
    public Guid? RecommendedByUserId { get; private set; }
    public DateTime? RecommendedAt { get; private set; }
    public DateOnly? DecisionDueDate { get; private set; }

    public ContractRenewalOption? Decision { get; private set; }
    public string? DecisionNote { get; private set; }
    public string? EmployeeMessage { get; private set; }
    public Guid? DecidedByUserId { get; private set; }
    public DateTime? DecidedAt { get; private set; }
    public DateTime? EmployeeNotifiedAt { get; private set; }

    public DateTime? ExpiringNotifiedAt { get; private set; }
    public DateTime? DecisionDueNotifiedAt { get; private set; }

    protected EmploymentContract()
    {
    }

    public EmploymentContract(
        Guid id,
        Guid? tenantId,
        Guid employeeId,
        string contractType,
        DateOnly startDate,
        DateOnly endDate,
        Guid? previousContractId,
        ContractStatus status)
        : base(id)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        ContractType = contractType;
        StartDate = startDate;
        EndDate = endDate;
        PreviousContractId = previousContractId;
        Status = status;
    }

    public void EnterAlertWindow(DateTime now)
    {
        Status = ContractStatus.PendingRecommendation;
        ExpiringNotifiedAt = now;
    }

    public void MarkDecisionDueNotified(DateTime now) => DecisionDueNotifiedAt = now;

    public void Recommend(string position, ContractRenewalOption recommendation, string? note, Guid userId, DateTime now, DateOnly dueDate)
    {
        EmployeePosition = position;
        Recommendation = recommendation;
        RecommendationNote = note;
        RecommendedByUserId = userId;
        RecommendedAt = now;
        DecisionDueDate = dueDate;
        Status = ContractStatus.PendingDecision;
    }

    public void Decide(ContractRenewalOption decision, string? note, string? employeeMessage, Guid userId, DateTime now)
    {
        Decision = decision;
        DecisionNote = note;
        EmployeeMessage = employeeMessage;
        DecidedByUserId = userId;
        DecidedAt = now;
        EmployeeNotifiedAt = now;
        Status = decision == ContractRenewalOption.Renew ? ContractStatus.Renewed : ContractStatus.NotRenewed;
    }
}

/// <summary>Daily work of the contracts flow. Safe to repeat: each notice has its own sent marker.</summary>
public class ContractAlertService : ITransientDependency
{
    private readonly IRepository<EmploymentContract, Guid> _contracts;
    private readonly HrSettingsStore _settings;
    private readonly ICompanyClock _companyClock;
    private readonly IdentityUserManager _userManager;
    private readonly HrNotificationPublisher _notifications;

    public ContractAlertService(
        IRepository<EmploymentContract, Guid> contracts,
        HrSettingsStore settings,
        ICompanyClock companyClock,
        IdentityUserManager userManager,
        HrNotificationPublisher notifications)
    {
        _contracts = contracts;
        _settings = settings;
        _companyClock = companyClock;
        _userManager = userManager;
        _notifications = notifications;
    }

    public virtual async Task ProcessAsync(DateTime now)
    {
        var today = await _companyClock.GetTodayAsync();
        var alertDays = (await _settings.PeekValueAsync<ContractSettings>()).AlertDays;
        var horizon = today.AddDays(alertDays);

        var entering = await _contracts.GetListAsync(x =>
            (x.Status == ContractStatus.Active && x.EndDate <= horizon) ||
            (x.Status == ContractStatus.PendingRecommendation && x.ExpiringNotifiedAt == null));
        var due = await _contracts.GetListAsync(x =>
            x.Status == ContractStatus.PendingDecision && x.DecisionDueNotifiedAt == null && x.DecisionDueDate != null && x.DecisionDueDate <= today);

        if (entering.Count == 0 && due.Count == 0)
        {
            return;
        }

        var hrUsers = (await _userManager.GetUsersInRoleAsync(HrRoles.HrManager)).Where(x => x.IsActive).Select(x => x.Id).ToList();

        foreach (var contract in entering)
        {
            contract.EnterAlertWindow(now);
            await _contracts.UpdateAsync(contract);
            foreach (var userId in hrUsers)
            {
                await _notifications.PublishAsync(userId, NotificationTemplateCode.ContractExpiring, NotificationResourceType.Contract, contract.Id);
            }
        }

        foreach (var contract in due)
        {
            contract.MarkDecisionDueNotified(now);
            await _contracts.UpdateAsync(contract);
            foreach (var userId in hrUsers)
            {
                await _notifications.PublishAsync(userId, NotificationTemplateCode.ContractDecisionDue, NotificationResourceType.Contract, contract.Id);
            }
        }
    }
}
