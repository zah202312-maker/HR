using System;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace HR_Mangment2.Identity;

public record MyAccountEmployeeInfo(EmployeeRefDto? Employee, int DirectReportCount);

/// <summary>The employee part of the account screen.</summary>
public interface IMyAccountEmployeeReader
{
    Task<MyAccountEmployeeInfo> GetAsync(Guid userId);
}

public class MyAccountEmployeeReader : IMyAccountEmployeeReader, ITransientDependency
{
    private readonly IRepository<Employee, Guid> _employees;
    private readonly EmployeeReferenceReader _references;

    public MyAccountEmployeeReader(IRepository<Employee, Guid> employees, EmployeeReferenceReader references)
    {
        _employees = employees;
        _references = references;
    }

    public async Task<MyAccountEmployeeInfo> GetAsync(Guid userId)
    {
        var employee = await _employees.FirstOrDefaultAsync(x => x.UserId == userId);
        if (employee == null)
        {
            return new MyAccountEmployeeInfo(null, 0);
        }

        var reports = await _employees.CountAsync(x => x.ManagerEmployeeId == employee.Id && x.Status != EmployeeStatus.Ended);
        return new MyAccountEmployeeInfo(await _references.ToRefAsync(employee), reports);
    }
}
