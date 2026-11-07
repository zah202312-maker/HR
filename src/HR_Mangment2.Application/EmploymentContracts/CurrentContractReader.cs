using System;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace HR_Mangment2.EmploymentContracts;

/// <summary>The contract covering today, for the employee file. Null when none covers it, which an expired contract does not.</summary>
public class CurrentContractReader : ITransientDependency
{
    private readonly IRepository<EmploymentContract, Guid> _contracts;
    private readonly ICompanyClock _companyClock;
    private readonly IAsyncQueryableExecuter _executer;

    public CurrentContractReader(IRepository<EmploymentContract, Guid> contracts, ICompanyClock companyClock, IAsyncQueryableExecuter executer)
    {
        _contracts = contracts;
        _companyClock = companyClock;
        _executer = executer;
    }

    public virtual async Task<EmployeeContractSummaryDto?> GetAsync(Guid employeeId)
    {
        var today = await _companyClock.GetTodayAsync();
        var contract = await _executer.FirstOrDefaultAsync((await _contracts.GetQueryableAsync())
            .Where(x => x.EmployeeId == employeeId && x.StartDate <= today && x.EndDate >= today)
            .OrderByDescending(x => x.StartDate));

        return contract == null
            ? null
            : new EmployeeContractSummaryDto { ContractId = contract.Id, StartDate = contract.StartDate, EndDate = contract.EndDate };
    }
}
