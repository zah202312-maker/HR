using System;
using Volo.Abp.Data;
using Volo.Abp.Identity;

namespace HR_Mangment2.Identity;

/// <summary>Columns added to AbpUsers, mapped in HR_Mangment2EfCoreEntityExtensionMappings.</summary>
public static class HrUserProperties
{
    public const string MustChangePassword = "MustChangePassword";
    public const string PreferredLanguage = "PreferredLanguage";
    public const string MfaEnabledAt = "MfaEnabledAt";

    /// <summary>Kept in ExtraProperties: while it is in the future the account email must not change.</summary>
    public const string PasswordResetLinkExpiresAt = "PasswordResetLinkExpiresAt";

    /// <summary>ABP requires an email on every user. Accounts with no real address get one on this reserved domain.</summary>
    private const string PlaceholderEmailDomain = "@hr.invalid";

    public static string PlaceholderEmail(string userName) => userName + PlaceholderEmailDomain;

    /// <summary>The real address, or null for an account that only has the placeholder.</summary>
    public static string? GetRealEmail(this IdentityUser user) =>
        string.IsNullOrWhiteSpace(user.Email) || user.Email.EndsWith(PlaceholderEmailDomain, StringComparison.OrdinalIgnoreCase)
            ? null
            : user.Email;

    public static DateTime? GetPasswordResetLinkExpiresAt(this IdentityUser user) => user.GetProperty<DateTime?>(PasswordResetLinkExpiresAt);

    public static void SetPasswordResetLinkExpiresAt(this IdentityUser user, DateTime? value) => user.SetProperty(PasswordResetLinkExpiresAt, value);

    public static bool GetMustChangePassword(this IdentityUser user) => user.GetProperty<bool>(MustChangePassword);

    public static void SetMustChangePassword(this IdentityUser user, bool value) => user.SetProperty(MustChangePassword, value);

    public static string GetPreferredLanguage(this IdentityUser user) => user.GetProperty<string>(PreferredLanguage) ?? "ar";

    public static void SetPreferredLanguage(this IdentityUser user, string value) => user.SetProperty(PreferredLanguage, value);

    public static DateTime? GetMfaEnabledAt(this IdentityUser user) => user.GetProperty<DateTime?>(MfaEnabledAt);

    public static void SetMfaEnabledAt(this IdentityUser user, DateTime? value) => user.SetProperty(MfaEnabledAt, value);
}
