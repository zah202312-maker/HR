using System;

namespace HR_Mangment2.Notifications;

public static class NotificationTexts
{
    public static string TitleKey(NotificationTemplateCode code) => $"Notification:{ToCamelCase(code)}:Title";

    public static string BodyKey(NotificationTemplateCode code) => $"Notification:{ToCamelCase(code)}:Body";

    private static string ToCamelCase(NotificationTemplateCode code)
    {
        var name = code.ToString();
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}
