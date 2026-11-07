using System;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace HR_Mangment2.Notifications;

[Authorize(HrPermissions.NotificationPermissions.Read)]
public class NotificationAppService : HR_Mangment2AppService, INotificationAppService
{
    private readonly IRepository<Notification, Guid> _notifications;
    private readonly IRepository<DeviceToken, Guid> _devices;

    public NotificationAppService(IRepository<Notification, Guid> notifications, IRepository<DeviceToken, Guid> devices)
    {
        _notifications = notifications;
        _devices = devices;
    }

    public virtual async Task<NotificationPagedResultDto> GetListAsync(NotificationListInput input)
    {
        var userId = CurrentUser.GetId();
        var query = (await _notifications.GetQueryableAsync()).Where(x => x.UserId == userId);

        if (input.IsRead.HasValue)
        {
            query = input.IsRead.Value ? query.Where(x => x.ReadAt != null) : query.Where(x => x.ReadAt == null);
        }

        var total = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new NotificationPagedResultDto { TotalCount = total, Items = items.Select(ToDto).ToList() };
    }

    public virtual async Task<NotificationUnreadCountDto> GetUnreadCountAsync()
    {
        var userId = CurrentUser.GetId();
        return new NotificationUnreadCountDto { Count = await _notifications.CountAsync(x => x.UserId == userId && x.ReadAt == null) };
    }

    public virtual async Task<NotificationDto> MarkReadAsync(Guid id)
    {
        var notification = await _notifications.FindAsync(id);
        if (notification == null || notification.UserId != CurrentUser.GetId())
        {
            throw new EntityNotFoundException(typeof(Notification), id);
        }

        notification.MarkRead(Clock.Now);
        await _notifications.UpdateAsync(notification);
        return ToDto(notification);
    }

    [Authorize(HrPermissions.NotificationPermissions.Device)]
    public virtual async Task RegisterDeviceAsync(DeviceTokenRegisterInputDto input)
    {
        var existing = await _devices.FirstOrDefaultAsync(x => x.Token == input.Token);
        if (existing == null)
        {
            await _devices.InsertAsync(new DeviceToken(GuidGenerator.Create(), CurrentTenant.Id, CurrentUser.GetId(), input.Token, input.Platform!.Value));
            return;
        }

        existing.AssignTo(CurrentUser.GetId(), input.Platform!.Value);
        await _devices.UpdateAsync(existing);
    }

    [Authorize(HrPermissions.NotificationPermissions.Device)]
    public virtual async Task UnregisterDeviceAsync(DeviceTokenUnregisterInputDto input)
    {
        var userId = CurrentUser.GetId();
        await _devices.DeleteAsync(x => x.Token == input.Token && x.UserId == userId);
    }

    private NotificationDto ToDto(Notification notification) => new()
    {
        Id = notification.Id,
        TemplateCode = notification.TemplateCode,
        ResourceType = notification.ResourceType,
        ResourceId = notification.ResourceId,
        Count = notification.Count,
        Title = L[NotificationTexts.TitleKey(notification.TemplateCode)],
        Body = L[NotificationTexts.BodyKey(notification.TemplateCode)],
        IsRead = notification.ReadAt != null,
        ReadAt = notification.ReadAt,
        CreationTime = notification.CreationTime
    };
}
