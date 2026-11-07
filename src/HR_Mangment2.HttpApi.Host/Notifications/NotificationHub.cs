using Microsoft.AspNetCore.Authorization;
using Volo.Abp.AspNetCore.SignalR;

namespace HR_Mangment2.Notifications;

/// <summary>Live updates: event ReceiveNotification with a NotificationDto payload.</summary>
[Authorize]
[HubRoute("/signalr-hubs/notification")]
public class NotificationHub : AbpHub
{
}
