using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace HR_Mangment2.Departments;

/// <summary>What other modules hold against a department.</summary>
public interface IDepartmentUsageReader
{
    /// <summary>Employees with status active or suspended, per department.</summary>
    Task<Dictionary<Guid, int>> GetEmployeeCountsAsync(IReadOnlyCollection<Guid> departmentIds);

    /// <summary>True if the department has employees in any status.</summary>
    Task<bool> HasEmployeesAsync(Guid departmentId);

    Task<bool> HasSharedCustodyAsync(Guid departmentId);
}

public class DepartmentUsageReader : IDepartmentUsageReader, ITransientDependency
{
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IRepository<HR_Mangment2.Store.Custody, Guid> _custodies;
    private readonly IAsyncQueryableExecuter _executer;

    public DepartmentUsageReader(IRepository<Employee, Guid> employees, IRepository<HR_Mangment2.Store.Custody, Guid> custodies, IAsyncQueryableExecuter executer)
    {
        _employees = employees;
        _custodies = custodies;
        _executer = executer;
    }

    public async Task<Dictionary<Guid, int>> GetEmployeeCountsAsync(IReadOnlyCollection<Guid> departmentIds)
    {
        var query = (await _employees.GetQueryableAsync())
            .Where(x => departmentIds.Contains(x.DepartmentId) && x.Status != EmployeeStatus.Ended)
            .GroupBy(x => x.DepartmentId)
            .Select(g => new { g.Key, Count = g.Count() });

        return (await _executer.ToListAsync(query)).ToDictionary(x => x.Key, x => x.Count);
    }

    public Task<bool> HasEmployeesAsync(Guid departmentId) => _employees.AnyAsync(x => x.DepartmentId == departmentId);

    public Task<bool> HasSharedCustodyAsync(Guid departmentId) =>
        _custodies.AnyAsync(x => x.DepartmentId == departmentId && x.Status != HR_Mangment2.Store.CustodyStatus.Returned);
}
