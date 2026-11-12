using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Common;
using HR_Mangment2.Localization;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace HR_Mangment2.Evaluations;

[Authorize(HrPermissions.EvaluationsPermissions.CycleRead)]
public class EvaluationCycleAppService : HR_Mangment2AppService, IEvaluationCycleAppService
{
    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["year"] = nameof(EvaluationCycle.Year),
        ["month"] = nameof(EvaluationCycle.Month),
        ["status"] = nameof(EvaluationCycle.Status)
    };

    private readonly IRepository<EvaluationCycle, Guid> _cycles;
    private readonly IRepository<DepartmentReport, Guid> _reports;
    private readonly MonthlyCompletionReader _completion;

    public EvaluationCycleAppService(IRepository<EvaluationCycle, Guid> cycles, IRepository<DepartmentReport, Guid> reports, MonthlyCompletionReader completion)
    {
        _cycles = cycles;
        _reports = reports;
        _completion = completion;
    }

    /// <summary>The open cycle if there is one, because on the first working days of a month the previous month is the one still being worked on.</summary>
    public virtual async Task<EvaluationCycleDto> GetCurrentAsync()
    {
        var query = await _cycles.GetQueryableAsync();
        var cycle = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Status == EvaluationCycleStatus.Open).OrderByDescending(x => x.Year).ThenByDescending(x => x.Month))
                    ?? await AsyncExecuter.FirstOrDefaultAsync(query.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month))
                    ?? throw new EntityNotFoundException(typeof(EvaluationCycle));

        return await ToDtoAsync(cycle);
    }

    public virtual async Task<EvaluationCyclePagedResultDto> GetListAsync(EvaluationCycleListInput input)
    {
        var query = (await _cycles.GetQueryableAsync()).WhereIf(input.Year != null, x => x.Year == input.Year);
        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(EvaluationCycle.Year) + " desc, " + nameof(EvaluationCycle.Month) + " desc", SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        var items = new List<EvaluationCycleDto>();
        foreach (var row in rows)
        {
            items.Add(await ToDtoAsync(row));
        }

        return new EvaluationCyclePagedResultDto { TotalCount = total, Items = items };
    }

    private async Task<EvaluationCycleDto> ToDtoAsync(EvaluationCycle cycle) => new()
    {
        Id = cycle.Id,
        Year = cycle.Year,
        Month = cycle.Month,
        Status = cycle.Status,
        OpensAt = cycle.OpensAt,
        DataFreezeAt = cycle.DataFreezeAt,
        DueAt = cycle.DueAt,
        EscalatesAt = cycle.EscalatesAt,
        ManagerProgress = await GetProgressAsync(cycle)
    };

    private async Task<EvaluationManagerProgressDto?> GetProgressAsync(EvaluationCycle cycle)
    {
        var me = CurrentUser.GetId();
        if (!await AuthorizationService.IsGrantedAsync(HrPermissions.EvaluationsPermissions.Manage))
        {
            return null;
        }

        var staff = await _completion.GetCurrentStaffAsync(me);
        if (staff.Count == 0)
        {
            return null;
        }

        var report = await _reports.FirstOrDefaultAsync(x => x.CycleId == cycle.Id && x.ManagerUserId == me);
        var completion = await _completion.ReadAsync(cycle, staff, report, Clock.Now);

        return new EvaluationManagerProgressDto
        {
            ExpectedCount = completion.Expected,
            SubmittedCount = completion.Submitted,
            RemainingCount = completion.Expected - completion.Submitted,
            ReportId = report?.Id,
            ReportStatus = report?.Status,
            IsCompleted = completion.IsCompleted,
            IsLate = completion.IsLate
        };
    }
}
