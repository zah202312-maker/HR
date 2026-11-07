using System.Threading.Tasks;
using HR_Mangment2.Localization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using Volo.Abp.DependencyInjection;

namespace HR_Mangment2.Notifications;

[ExposeServices(typeof(INotificationChannel))]
public class SignalRNotificationChannel : INotificationChannel, ITransientDependency
{
    public const string EventName = "ReceiveNotification";

    private readonly IHubContext<NotificationHub> _hub;
    private readonly IStringLocalizer<HR_Mangment2Resource> _localizer;

    public SignalRNotificationChannel(IHubContext<NotificationHub> hub, IStringLocalizer<HR_Mangment2Resource> localizer)
    {
        _hub = hub;
        _localizer = localizer;
    }

    public Task SendAsync(Notification notification)
    {
        return _hub.Clients.User(notification.UserId.ToString()).SendAsync(EventName, new NotificationDto
        {
            Id = notification.Id,
            TemplateCode = notification.TemplateCode,
            ResourceType = notification.ResourceType,
            ResourceId = notification.ResourceId,
            Count = notification.Count,
            Title = _localizer[NotificationTexts.TitleKey(notification.TemplateCode)],
            Body = _localizer[NotificationTexts.BodyKey(notification.TemplateCode)],
            CreationTime = notification.CreationTime
        });
    }
}
