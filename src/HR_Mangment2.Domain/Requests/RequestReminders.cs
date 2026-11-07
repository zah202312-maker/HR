using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Common;
using HR_Mangment2.Employees;
using HR_Mangment2.Notifications;
using HR_Mangment2.Settings;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace HR_Mangment2.Requests;

/// <summary>A reminder never approves, rejects, escalates or changes a status: it only tells the current approver again.</summary>
public class RequestApprovalReminderService : ITransientDependency
{
    private readonly IRepository<Request, Guid> _requests;
    private readonly RequestWorkflowService _workflow;
    private readonly WorkCalendarService _calendar;
    private readonly HrSettingsStore _settings;
    private readonly HrNotificationPublisher _notifications;
    private readonly ICompanyClock _companyClock;

    public RequestApprovalReminderService(
        IRepository<Request, Guid> requests,
        RequestWorkflowService workflow,
        WorkCalendarService calendar,
        HrSettingsStore settings,
        HrNotificationPublisher notifications,
        ICompanyClock companyClock)
    {
        _requests = requests;
        _workflow = workflow;
        _calendar = calendar;
        _settings = settings;
        _notifications = notifications;
        _companyClock = companyClock;
    }

    /// <returns>How many requests had a reminder sent.</returns>
    public virtual async Task<int> SendDueAsync(DateTime now)
    {
        var interval = (await _settings.PeekValueAsync<ReminderSettings>()).ApprovalReminderIntervalWorkingDays;
        if (interval is null or <= 0)
        {
            return 0;
        }

        var zone = await _companyClock.GetTimeZoneAsync();
        var today = CompanyClock.ToLocalDate(now, zone);
        var pending = await _requests.GetListAsync(x => x.CurrentStepOrder != null);
        var sent = 0;

        foreach (var request in pending)
        {
            var since = CompanyClock.ToLocalDate(request.LastReminderAt ?? request.CurrentStepSince, zone);
            if (await _calendar.CountWorkingDaysAfterAsync(since, today) < interval)
            {
                continue;
            }

            foreach (var approver in await _workflow.GetCurrentApproverUserIdsAsync(request))
            {
                await _notifications.PublishAsync(approver, NotificationTemplateCode.RequestApprovalReminder, NotificationResourceType.Request, request.Id);
            }

            request.MarkReminded(now);
            await _requests.UpdateAsync(request);
            sent++;
        }

        return sent;
    }
}

public class RequestReminderWorker : AsyncPeriodicBackgroundWorkerBase
{
    /// <summary>A reminder falling due overnight waits until the morning instead of arriving at midnight.</summary>
    private const int FirstHourOfDay = 8;

    public RequestReminderWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = (int)TimeSpan.FromHours(1).TotalMilliseconds;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var services = workerContext.ServiceProvider;
        var unitOfWorkManager = services.GetRequiredService<IUnitOfWorkManager>();
        var currentTenant = services.GetRequiredService<ICurrentTenant>();
        var now = services.GetRequiredService<IClock>().Now;

        List<Guid> tenantIds;
        using (var read = unitOfWorkManager.Begin(requiresNew: true))
        {
            tenantIds = (await services.GetRequiredService<ITenantRepository>().GetListAsync()).Select(x => x.Id).ToList();
            await read.CompleteAsync();
        }

        foreach (var tenantId in tenantIds)
        {
            using (currentTenant.Change(tenantId))
            using (var unitOfWork = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                var zone = await services.GetRequiredService<ICompanyClock>().GetTimeZoneAsync();
                if (TimeZoneInfo.ConvertTimeFromUtc(now, zone).Hour >= FirstHourOfDay)
                {
                    await services.GetRequiredService<RequestApprovalReminderService>().SendDueAsync(now);
                }

                await unitOfWork.CompleteAsync();
            }
        }
    }
}
