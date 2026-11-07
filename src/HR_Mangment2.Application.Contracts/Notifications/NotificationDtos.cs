using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.Notifications;

public class NotificationDto
{
    public Guid Id { get; set; }
    public NotificationTemplateCode TemplateCode { get; set; }
    public NotificationResourceType ResourceType { get; set; }
    public Guid? ResourceId { get; set; }
    public int? Count { get; set; }
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreationTime { get; set; }
}

public class NotificationPagedResultDto
{
    public long TotalCount { get; set; }
    public List<NotificationDto> Items { get; set; } = new();
}

public class NotificationUnreadCountDto
{
    public int Count { get; set; }
}

public class NotificationListInput : PagedAndSortedResultRequestDto
{
    public bool? IsRead { get; set; }
}

public class DeviceTokenRegisterInputDto
{
    [Required, StringLength(4096, MinimumLength = 1)]
    public string Token { get; set; } = null!;

    [Required]
    public DevicePlatform? Platform { get; set; }
}

public class DeviceTokenUnregisterInputDto
{
    [Required, StringLength(4096, MinimumLength = 1)]
    public string Token { get; set; } = null!;
}

public interface INotificationAppService : IApplicationService
{
    Task<NotificationPagedResultDto> GetListAsync(NotificationListInput input);
    Task<NotificationUnreadCountDto> GetUnreadCountAsync();
    Task<NotificationDto> MarkReadAsync(Guid id);
    Task RegisterDeviceAsync(DeviceTokenRegisterInputDto input);
    Task UnregisterDeviceAsync(DeviceTokenUnregisterInputDto input);
}
