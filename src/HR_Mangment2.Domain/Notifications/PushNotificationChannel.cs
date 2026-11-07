using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace HR_Mangment2.Notifications;

/// <summary>Sends to FCM. Firebase credentials are per environment, so development uses the logging sender.</summary>
public interface IPushSender
{
    Task SendAsync(string deviceToken, Notification notification);
}

public class NullPushSender : IPushSender, ISingletonDependency
{
    public Task SendAsync(string deviceToken, Notification notification) => Task.CompletedTask;
}

[ExposeServices(typeof(INotificationChannel))]
public class PushNotificationChannel : INotificationChannel, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPushSender _sender;

    public PushNotificationChannel(IServiceProvider serviceProvider, IPushSender sender)
    {
        _serviceProvider = serviceProvider;
        _sender = sender;
    }

    public bool IsQueued => true;

    public async Task SendAsync(Notification notification)
    {
        var tokens = (IReadOnlyRepository<DeviceToken, Guid>)_serviceProvider.GetService(typeof(IReadOnlyRepository<DeviceToken, Guid>))!;
        foreach (var device in await tokens.GetListAsync(x => x.UserId == notification.UserId))
        {
            await _sender.SendAsync(device.Token, notification);
        }
    }
}
