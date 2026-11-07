using System;
using System.Globalization;
using System.Threading.Tasks;
using HR_Mangment2.Identity;
using HR_Mangment2.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Emailing;
using Volo.Abp.Identity;
using Volo.Abp.Localization;

namespace HR_Mangment2.Notifications;

/// <summary>Sends the template text in the recipient's language (document 5.17).</summary>
[ExposeServices(typeof(INotificationChannel))]
public class EmailNotificationChannel : INotificationChannel, ITransientDependency
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EmailNotificationChannel(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public bool IsQueued => true;

    public async Task SendAsync(Notification notification)
    {
        using var scope = _scopeFactory.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<IIdentityUserRepository>();
        var user = await users.FindAsync(notification.UserId, includeDetails: false);
        var email = user?.GetRealEmail();
        if (user == null || email == null)
        {
            return;
        }

        var localizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<HR_Mangment2Resource>>();
        using (CultureHelper.Use(user.GetPreferredLanguage()))
        {
            await scope.ServiceProvider.GetRequiredService<IEmailSender>().SendAsync(
                email,
                localizer[NotificationTexts.TitleKey(notification.TemplateCode)],
                localizer[NotificationTexts.BodyKey(notification.TemplateCode)]);
        }
    }
}
