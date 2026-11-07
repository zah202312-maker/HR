using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace HR_Mangment2.WorkShifts;

public interface IWorkShiftUsageReader
{
    /// <summary>Employees currently assigned, per shift.</summary>
    Task<Dictionary<Guid, int>> GetAssignedEmployeeCountsAsync(IReadOnlyCollection<Guid> shiftIds);
}

public class WorkShiftUsageReader : IWorkShiftUsageReader, ITransientDependency
{
    private readonly IRepository<EmployeeShiftAssignment, Guid> _assignments;
    private readonly IAsyncQueryableExecuter _executer;

    public WorkShiftUsageReader(IRepository<EmployeeShiftAssignment, Guid> assignments, IAsyncQueryableExecuter executer)
    {
        _assignments = assignments;
        _executer = executer;
    }

    public async Task<Dictionary<Guid, int>> GetAssignedEmployeeCountsAsync(IReadOnlyCollection<Guid> shiftIds)
    {
        var query = (await _assignments.GetQueryableAsync())
            .Where(x => shiftIds.Contains(x.WorkShiftId) && x.EndDate == null)
            .GroupBy(x => x.WorkShiftId)
            .Select(g => new { g.Key, Count = g.Count() });

        return (await _executer.ToListAsync(query)).ToDictionary(x => x.Key, x => x.Count);
    }
}
