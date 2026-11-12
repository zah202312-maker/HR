using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace HR_Mangment2.Employees;

/// <summary>
/// Records the salary agreed at hiring as the employee's first salary version (decision 2026-11-10). The finance module
/// implements it, because the main project must not reference that module.
/// </summary>
public interface IInitialSalaryGateway
{
    /// <param name="currency">ISO 4217, or null for the company's default currency.</param>
    Task RecordAsync(Employee employee, decimal baseSalary, string? currency);
}

/// <summary>Without the finance module there is no salary to record.</summary>
public class NullInitialSalaryGateway : IInitialSalaryGateway, ITransientDependency
{
    public Task RecordAsync(Employee employee, decimal baseSalary, string? currency) => Task.CompletedTask;
}
