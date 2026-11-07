using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace HR_Mangment2.Notifications;

public interface INotificationChannel
{
    /// <summary>Email and phone go through a background job queued with the business change, so a crash cannot lose them (Q9).</summary>
    bool IsQueued => false;

    Task SendAsync(Notification notification);
}

[BackgroundJobName("HrNotificationDelivery")]
public class NotificationDeliveryArgs
{
    public Guid NotificationId { get; set; }
    public Guid? TenantId { get; set; }
}

/// <summary>A failure throws, so the job manager tries again. The log carries the template, never the content.</summary>
public class NotificationDeliveryJob : AsyncBackgroundJob<NotificationDeliveryArgs>, ITransientDependency
{
    private readonly IRepository<Notification, Guid> _repository;
    private readonly IEnumerable<INotificationChannel> _channels;
    private readonly ICurrentTenant _currentTenant;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public NotificationDeliveryJob(IRepository<Notification, Guid> repository, IEnumerable<INotificationChannel> channels, ICurrentTenant currentTenant, IUnitOfWorkManager unitOfWorkManager)
    {
        _repository = repository;
        _channels = channels;
        _currentTenant = currentTenant;
        _unitOfWorkManager = unitOfWorkManager;
    }

    public override async Task ExecuteAsync(NotificationDeliveryArgs args)
    {
        using (_currentTenant.Change(args.TenantId))
        {
            Notification? notification;
            using (var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true))
            {
                notification = await _repository.FindAsync(args.NotificationId);
                await unitOfWork.CompleteAsync();
            }

            if (notification == null)
            {
                return;
            }

            foreach (var channel in _channels.Where(x => x.IsQueued))
            {
                await channel.SendAsync(notification);
            }
        }
    }
}

/// <summary>
/// The in-app row commits with the business change, so it cannot be lost. The other channels run after the
/// commit, and a channel failure is logged without failing the operation.
/// </summary>
public class HrNotificationPublisher : ITransientDependency
{
    private readonly IRepository<Notification, Guid> _repository;
    private readonly IEnumerable<INotificationChannel> _channels;
    private readonly IBackgroundJobManager _jobs;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<HrNotificationPublisher> _logger;

    public HrNotificationPublisher(
        IRepository<Notification, Guid> repository,
        IEnumerable<INotificationChannel> channels,
        IBackgroundJobManager jobs,
        IUnitOfWorkManager unitOfWorkManager,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        ILogger<HrNotificationPublisher> logger)
    {
        _repository = repository;
        _channels = channels;
        _jobs = jobs;
        _unitOfWorkManager = unitOfWorkManager;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public virtual async Task PublishAsync(
        Guid userId,
        NotificationTemplateCode template,
        NotificationResourceType resourceType = NotificationResourceType.None,
        Guid? resourceId = null,
        int? count = null)
    {
        var notification = new Notification(_guidGenerator.Create(), _currentTenant.Id, userId, template, resourceType, resourceId, count);
        await _repository.InsertAsync(notification);
        await _jobs.EnqueueAsync(new NotificationDeliveryArgs { NotificationId = notification.Id, TenantId = _currentTenant.Id }, BackgroundJobPriority.Low);

        var unitOfWork = _unitOfWorkManager.Current;
        if (unitOfWork == null)
        {
            await SendToChannelsAsync(notification);
            return;
        }

        unitOfWork.OnCompleted(() => SendToChannelsAsync(notification));
    }

    private async Task SendToChannelsAsync(Notification notification)
    {
        foreach (var channel in _channels.Where(x => !x.IsQueued))
        {
            try
            {
                await channel.SendAsync(notification);
            }
            catch (Exception ex)
            {
                // Only the type and template go to the log, never content.
                _logger.LogWarning(ex, "Notification channel {Channel} failed for template {Template}.", channel.GetType().Name, notification.TemplateCode);
            }
        }
    }
}
