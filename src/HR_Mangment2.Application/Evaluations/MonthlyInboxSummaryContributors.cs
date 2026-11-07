using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Requests;
using HR_Mangment2.Store;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Linq;

namespace HR_Mangment2.Evaluations;

/// <summary>What the monthly cycle and the circulars leave waiting for a user. None of these can be delegated, so the delegated count is always zero.</summary>
public class MonthlyInboxSummaryContributor : IInboxSummaryContributor, ITransientDependency
{
    private readonly IRepository<Evaluation, Guid> _evaluations;
    private readonly IRepository<EvaluationCycle, Guid> _cycles;
    private readonly IRepository<DepartmentReport, Guid> _reports;
    private readonly IRepository<CircularRecipient, Guid> _recipients;
    private readonly IRepository<Custody, Guid> _custodies;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly CustodyDepartmentScope _custodyScope;
    private readonly IIdentityUserRepository _users;
    private readonly IAsyncQueryableExecuter _executer;

    public MonthlyInboxSummaryContributor(
        IRepository<Evaluation, Guid> evaluations,
        IRepository<EvaluationCycle, Guid> cycles,
        IRepository<DepartmentReport, Guid> reports,
        IRepository<CircularRecipient, Guid> recipients,
        IRepository<Custody, Guid> custodies,
        IRepository<Employee, Guid> employees,
        CustodyDepartmentScope custodyScope,
        IIdentityUserRepository users,
        IAsyncQueryableExecuter executer)
    {
        _evaluations = evaluations;
        _cycles = cycles;
        _reports = reports;
        _recipients = recipients;
        _custodies = custodies;
        _employees = employees;
        _custodyScope = custodyScope;
        _users = users;
        _executer = executer;
    }

    public virtual async Task<IReadOnlyCollection<InboxSummaryItemDto>> GetAsync(Guid userId)
    {
        var me = await _employees.FirstOrDefaultAsync(x => x.UserId == userId);
        var roles = await _users.GetRoleNamesAsync(userId);
        var items = new List<InboxSummaryItemDto>();

        if (roles.Contains(HrRoles.DepartmentManager) && me != null)
        {
            var team = await _executer.ToListAsync((await _employees.GetQueryableAsync()).Where(x => x.ManagerEmployeeId == me.Id).Select(x => x.Id));
            var openCycles = await _executer.ToListAsync((await _cycles.GetQueryableAsync()).Where(x => x.Status == EvaluationCycleStatus.Open).Select(x => x.Id));

            items.Add(Item(InboxCategory.Evaluation, await _evaluations.CountAsync(x => openCycles.Contains(x.CycleId) && team.Contains(x.EmployeeId) && x.Status == EvaluationStatus.Draft)));
            items.Add(Item(InboxCategory.DepartmentReport, await _reports.CountAsync(x => x.ManagerUserId == userId && x.Status == DepartmentReportStatus.Draft)));
        }

        if (roles.Contains(HrRoles.HrManager))
        {
            var ownId = me?.Id ?? Guid.Empty;
            items.Add(Item(InboxCategory.EvaluationObjection, await _evaluations.CountAsync(x => x.ObjectionStatus == EvaluationObjectionStatus.Pending && x.EmployeeId != ownId)));
        }

        if (me != null)
        {
            items.Add(Item(InboxCategory.EvaluationAcknowledgement, await _evaluations.CountAsync(x => x.EmployeeId == me.Id && x.Status == EvaluationStatus.Submitted)));
        }

        items.Add(Item(InboxCategory.CircularAcknowledgement, await _recipients.CountAsync(x => x.UserId == userId && x.AcknowledgedAt == null)));

        var departments = await _custodyScope.GetManagedDepartmentIdsAsync(userId);
        var ownEmployeeId = me?.Id;
        items.Add(Item(InboxCategory.CustodyAcknowledgement, await _custodies.CountAsync(x =>
            x.Status == CustodyStatus.PendingAcknowledgement &&
            ((x.EmployeeId != null && x.EmployeeId == ownEmployeeId) || (x.DepartmentId != null && departments.Contains(x.DepartmentId.Value))))));

        return items;
    }

    private static InboxSummaryItemDto Item(InboxCategory category, int count) => new() { Category = category, Count = count, DelegatedCount = 0 };
}
