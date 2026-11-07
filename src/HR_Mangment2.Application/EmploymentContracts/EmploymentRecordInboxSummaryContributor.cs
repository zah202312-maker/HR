using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Appreciation;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Requests;
using HR_Mangment2.Warnings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace HR_Mangment2.EmploymentContracts;

/// <summary>Counts only, as the summary requires: what waits on this user from contracts, warnings and appreciation letters.</summary>
public class EmploymentRecordInboxSummaryContributor : IInboxSummaryContributor, ITransientDependency
{
    private static readonly WarningStatus[] HrDecisionStatuses = { WarningStatus.Recommended, WarningStatus.Responded, WarningStatus.ResponseExpired };
    private static readonly ContractStatus[] HrContractStatuses = { ContractStatus.PendingRecommendation, ContractStatus.PendingDecision };

    private readonly IRepository<Warning, Guid> _warnings;
    private readonly IRepository<EmploymentContract, Guid> _contracts;
    private readonly IRepository<AppreciationLetter, Guid> _letters;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IIdentityUserRepository _users;
    private readonly ICompanyClock _companyClock;

    public EmploymentRecordInboxSummaryContributor(
        IRepository<Warning, Guid> warnings,
        IRepository<EmploymentContract, Guid> contracts,
        IRepository<AppreciationLetter, Guid> letters,
        IRepository<Employee, Guid> employees,
        IIdentityUserRepository users,
        ICompanyClock companyClock)
    {
        _warnings = warnings;
        _contracts = contracts;
        _letters = letters;
        _employees = employees;
        _users = users;
        _companyClock = companyClock;
    }

    public virtual async Task<IReadOnlyCollection<InboxSummaryItemDto>> GetAsync(Guid userId)
    {
        var items = new List<InboxSummaryItemDto>();
        void Add(InboxCategory category, long count)
        {
            if (count > 0)
            {
                items.Add(new InboxSummaryItemDto { Category = category, Count = (int)count, DelegatedCount = 0 });
            }
        }

        var me = await _employees.FirstOrDefaultAsync(x => x.UserId == userId);
        if (me != null)
        {
            var today = await _companyClock.GetTodayAsync();
            Add(InboxCategory.WarningResponse, await _warnings.LongCountAsync(x => x.EmployeeId == me.Id && x.Status == WarningStatus.Notified && x.ResponseDueDate >= today));
            Add(InboxCategory.AppreciationLetterAcknowledgement, await _letters.LongCountAsync(x => x.EmployeeId == me.Id && x.AcknowledgedAt == null));
        }

        var roles = await _users.GetRoleNamesAsync(userId);
        var ownId = me?.Id ?? Guid.Empty;
        long decisions = 0;

        if (roles.Contains(HrRoles.HrManager))
        {
            decisions += await _warnings.LongCountAsync(x => HrDecisionStatuses.Contains(x.Status) && x.EmployeeId != ownId && x.RecommendedByUserId != userId);
            Add(InboxCategory.ContractDecision, await _contracts.LongCountAsync(x => HrContractStatuses.Contains(x.Status) && x.EmployeeId != ownId));
        }

        if (roles.Contains(HrRoles.ExecutiveDirector))
        {
            decisions += await _warnings.LongCountAsync(x => x.Status == WarningStatus.Objected);
        }

        Add(InboxCategory.WarningDecision, decisions);

        return items;
    }
}
