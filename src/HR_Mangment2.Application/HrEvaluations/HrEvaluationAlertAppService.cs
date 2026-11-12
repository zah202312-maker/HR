using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Localization;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace HR_Mangment2.HrEvaluations;

/// <summary>Read only list of the low average alerts; the alerts are made by HrEvaluationAlertService (decision 2026-11-11).</summary>
[Authorize(HrPermissions.HrEvaluationPermissions.Manage)]
public class HrEvaluationAlertAppService : HR_Mangment2AppService, IHrEvaluationAlertAppService
{
    private readonly IRepository<HrEvaluationAlert, Guid> _alerts;
    private readonly IRepository<HrEvaluationCriterion, Guid> _criteria;
    private readonly EmployeeReferenceReader _references;

    public HrEvaluationAlertAppService(
        IRepository<HrEvaluationAlert, Guid> alerts,
        IRepository<HrEvaluationCriterion, Guid> criteria,
        EmployeeReferenceReader references)
    {
        _alerts = alerts;
        _criteria = criteria;
        _references = references;
    }

    public virtual async Task<HrEvaluationAlertPagedResultDto> GetListAsync(HrEvaluationAlertListInput input)
    {
        var query = (await _alerts.GetQueryableAsync())
            .WhereIf(input.EmployeeId != null, x => x.EmployeeId == input.EmployeeId)
            .WhereIf(input.From != null, x => x.PeriodTo >= input.From)
            .WhereIf(input.To != null, x => x.PeriodTo <= input.To);

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query
            .OrderByDescending(x => x.PeriodTo).ThenBy(x => x.AverageScore).ThenBy(x => x.Id)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount));
        return new HrEvaluationAlertPagedResultDto { TotalCount = total, Items = await ToDtosAsync(rows) };
    }

    public virtual async Task<HrEvaluationAlertDto> GetAsync(Guid id)
    {
        var alert = await _alerts.FindAsync(id) ?? throw new EntityNotFoundException(typeof(HrEvaluationAlert), id);
        return (await ToDtosAsync(new[] { alert }))[0];
    }

    private async Task<List<HrEvaluationAlertDto>> ToDtosAsync(IReadOnlyCollection<HrEvaluationAlert> rows)
    {
        var employees = await _references.GetRefsAsync(rows.Select(x => x.EmployeeId));
        var criterionIds = rows.Select(x => x.CriterionId).Distinct().ToList();
        var criteria = (await _criteria.GetListAsync(x => criterionIds.Contains(x.Id))).ToDictionary(x => x.Id);

        return rows.Select(x => new HrEvaluationAlertDto
        {
            Id = x.Id,
            Employee = employees[x.EmployeeId],
            CriterionId = x.CriterionId,
            CriterionName = LocalizedName.Pick(criteria[x.CriterionId].NameAr, criteria[x.CriterionId].NameEn),
            AverageScore = x.AverageScore,
            EvaluationsCount = x.EvaluationsCount,
            Threshold = x.Threshold,
            PeriodFrom = x.PeriodFrom,
            PeriodTo = x.PeriodTo,
            CreationTime = x.CreationTime
        }).ToList();
    }
}
