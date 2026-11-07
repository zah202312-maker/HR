using System;
using System.Threading.Tasks;
using HR_Mangment2.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace HR_Mangment2.Controllers.Notifications;

[Route("api/app")]
public class NotificationController : HR_Mangment2Controller
{
    private readonly INotificationAppService _service;

    public NotificationController(INotificationAppService service)
    {
        _service = service;
    }

    [HttpGet("notification", Name = "Notification_List")]
    public Task<NotificationPagedResultDto> GetListAsync([FromQuery] NotificationListInput input) => _service.GetListAsync(input);

    [HttpGet("notification/unread-count", Name = "Notification_GetUnreadCount")]
    public Task<NotificationUnreadCountDto> GetUnreadCountAsync() => _service.GetUnreadCountAsync();

    [HttpPost("notification/{id:guid}/read", Name = "Notification_MarkRead")]
    public Task<NotificationDto> MarkReadAsync(Guid id) => _service.MarkReadAsync(id);

    [HttpPost("device-token", Name = "DeviceToken_Register")]
    public async Task<IActionResult> RegisterAsync([FromBody] DeviceTokenRegisterInputDto input)
    {
        await _service.RegisterDeviceAsync(input);
        return NoContent();
    }

    [HttpPost("device-token/unregister", Name = "DeviceToken_Unregister")]
    public async Task<IActionResult> UnregisterAsync([FromBody] DeviceTokenUnregisterInputDto input)
    {
        await _service.UnregisterDeviceAsync(input);
        return NoContent();
    }
}
