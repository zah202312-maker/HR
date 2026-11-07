using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Files;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace HR_Mangment2.Leaves;

public class LeaveMaintenanceService : ITransientDependency
{
    private readonly IRepository<EmployeeLeaveYear, Guid> _years;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IRepository<LeaveAttachment, Guid> _attachments;
    private readonly IBlobContainer<HrFilesContainer> _files;
    private readonly LeaveBalanceManager _balances;
    private readonly ICompanyClock _companyClock;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IClock _clock;

    public LeaveMaintenanceService(
        IRepository<EmployeeLeaveYear, Guid> years,
        IRepository<Employee, Guid> employees,
        IRepository<LeaveAttachment, Guid> attachments,
        IBlobContainer<HrFilesContainer> files,
        LeaveBalanceManager balances,
        ICompanyClock companyClock,
        IUnitOfWorkManager unitOfWorkManager,
        IClock clock)
    {
        _years = years;
        _employees = employees;
        _attachments = attachments;
        _files = files;
        _balances = balances;
        _companyClock = companyClock;
        _unitOfWorkManager = unitOfWorkManager;
        _clock = clock;
    }

    /// <summary>Each employee in their own transaction, so one failure does not hold back everyone else's new year.</summary>
    /// <param name="today">The date to roll up to. Null means today in the company time zone.</param>
    public virtual async Task RollOverDueYearsAsync(DateOnly? today = null)
    {
        var date = today ?? await _companyClock.GetTodayAsync();

        List<Guid> due;
        using (var read = _unitOfWorkManager.Begin(requiresNew: true))
        {
            due = (await _years.GetListAsync(x => x.IsGranted && !x.IsClosed && x.YearEnd < date)).Select(x => x.EmployeeId).Distinct().ToList();
            await read.CompleteAsync();
        }

        foreach (var employeeId in due)
        {
            using var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
            var employee = await _employees.GetAsync(employeeId);
            await _balances.RollOverAsync(employee, date);
            await unitOfWork.CompleteAsync();
        }
    }

    public virtual async Task DeleteUnusedAttachmentsAsync()
    {
        var cutoff = _clock.Now - LeaveAttachment.UnusedLifetime;

        using var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        foreach (var attachment in await _attachments.GetListAsync(x => x.LeaveRequestId == null && x.CreationTime < cutoff))
        {
            await _files.DeleteAsync(attachment.BlobName);
            await _attachments.DeleteAsync(attachment);
        }

        await unitOfWork.CompleteAsync();
    }
}

/// <summary>Hourly, so a new leave year opens on its first morning. Safe to repeat.</summary>
public class LeaveMaintenanceWorker : AsyncPeriodicBackgroundWorkerBase
{
    public LeaveMaintenanceWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = (int)TimeSpan.FromHours(1).TotalMilliseconds;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var services = workerContext.ServiceProvider;
        var unitOfWorkManager = services.GetRequiredService<IUnitOfWorkManager>();
        var currentTenant = services.GetRequiredService<ICurrentTenant>();

        List<Guid> tenantIds;
        using (var read = unitOfWorkManager.Begin(requiresNew: true))
        {
            tenantIds = (await services.GetRequiredService<ITenantRepository>().GetListAsync()).Select(x => x.Id).ToList();
            await read.CompleteAsync();
        }

        foreach (var tenantId in tenantIds)
        {
            using (currentTenant.Change(tenantId))
            {
                var maintenance = services.GetRequiredService<LeaveMaintenanceService>();
                await maintenance.RollOverDueYearsAsync();
                await maintenance.DeleteUnusedAttachmentsAsync();
            }
        }
    }
}
