using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.EmploymentContracts;
using HR_Mangment2.Localization;
using HR_Mangment2.Warnings;
using Microsoft.Extensions.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace HR_Mangment2.Reports;

/// <summary>Both report rows are free of financial data and of the incident description, so an export cannot carry them either.</summary>
public class EmploymentRecordReportQueries : ITransientDependency
{
    private static readonly Dictionary<string, string> ContractSort = new()
    {
        ["typeName"] = nameof(EmploymentContract.ContractType),
        ["startDate"] = nameof(EmploymentContract.StartDate),
        ["endDate"] = nameof(EmploymentContract.EndDate)
    };

    private static readonly Dictionary<string, string> WarningSort = new()
    {
        ["incidentDate"] = nameof(Warning.IncidentDate)
    };

    private readonly IRepository<EmploymentContract, Guid> _contracts;
    private readonly IRepository<Warning, Guid> _warnings;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly EmployeeReferenceReader _references;
    private readonly IAsyncQueryableExecuter _executer;
    private readonly IStringLocalizer<HR_Mangment2Resource> _l;

    public EmploymentRecordReportQueries(
        IRepository<EmploymentContract, Guid> contracts,
        IRepository<Warning, Guid> warnings,
        IRepository<Employee, Guid> employees,
        EmployeeReferenceReader references,
        IAsyncQueryableExecuter executer,
        IStringLocalizer<HR_Mangment2Resource> localizer)
    {
        _contracts = contracts;
        _warnings = warnings;
        _employees = employees;
        _references = references;
        _executer = executer;
        _l = localizer;
    }

    public virtual async Task<HrContractReportRowPagedResultDto> GetHrContractsAsync(DateOnly? endFrom, DateOnly? endTo, Guid? departmentId, Guid? employeeId, ReportPage page)
    {
        var scope = await EmployeeScopeAsync(departmentId, employeeId);
        var query = (await _contracts.GetQueryableAsync())
            .Where(x => scope.Contains(x.EmployeeId))
            .Where(x => endFrom == null || x.EndDate >= endFrom)
            .Where(x => endTo == null || x.EndDate <= endTo);

        var total = await _executer.LongCountAsync(query);
        var rows = await _executer.ToListAsync(query
            .ApplySorting(page.Sorting, nameof(EmploymentContract.EndDate), ContractSort)
            .Skip(page.Skip)
            .Take(page.Take));
        var employees = await _references.GetRefsAsync(rows.Select(x => x.EmployeeId));

        return new HrContractReportRowPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => new HrContractReportRowDto
            {
                ContractId = x.Id,
                Employee = employees[x.EmployeeId],
                TypeName = x.ContractType,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                StatusName = _l["Contract:Status:" + JsonNamingPolicy.CamelCase.ConvertName(x.Status.ToString())]
            }).ToList()
        };
    }

    public virtual async Task<HrWarningReportRowPagedResultDto> GetHrWarningsAsync(DateOnly from, DateOnly to, Guid? departmentId, Guid? employeeId, ReportPage page)
    {
        var scope = await EmployeeScopeAsync(departmentId, employeeId);
        var query = (await _warnings.GetQueryableAsync())
            .Where(x => scope.Contains(x.EmployeeId) && x.IncidentDate >= from && x.IncidentDate <= to);

        var total = await _executer.LongCountAsync(query);
        var rows = await _executer.ToListAsync(query
            .ApplySorting(page.Sorting, nameof(Warning.IncidentDate) + " desc", WarningSort)
            .Skip(page.Skip)
            .Take(page.Take));
        var employees = await _references.GetRefsAsync(rows.Select(x => x.EmployeeId));

        return new HrWarningReportRowPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => new HrWarningReportRowDto
            {
                WarningId = x.Id,
                Employee = employees[x.EmployeeId],
                LevelName = _l["Warning:Level:" + JsonNamingPolicy.CamelCase.ConvertName((x.IssuedLevel ?? x.RecommendedLevel).ToString())],
                IncidentDate = x.IncidentDate,
                StatusName = _l["Warning:Status:" + JsonNamingPolicy.CamelCase.ConvertName(x.Status.ToString())]
            }).ToList()
        };
    }

    private async Task<IQueryable<Guid>> EmployeeScopeAsync(Guid? departmentId, Guid? employeeId) =>
        (await _employees.GetQueryableAsync())
            .Where(x => departmentId == null || x.DepartmentId == departmentId)
            .Where(x => employeeId == null || x.Id == employeeId)
            .Select(x => x.Id);
}
