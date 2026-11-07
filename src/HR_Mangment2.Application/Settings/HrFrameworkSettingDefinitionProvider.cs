using Volo.Abp.Localization;
using Volo.Abp.Settings;

namespace HR_Mangment2.Settings;

/// <summary>
/// Changes defaults of framework settings. It lives here because it must run after the
/// Account module has defined its settings.
/// </summary>
public class HrFrameworkSettingDefinitionProvider : SettingDefinitionProvider
{
    public const string SelfRegistrationSettingName = "Abp.Account.IsSelfRegistrationEnabled";

    public override void Define(ISettingDefinitionContext context)
    {
        SetDefault(context, LocalizationSettingNames.DefaultLanguage, "ar");
        SetDefault(context, "Abp.Identity.Password.RequiredLength", "8");

        // Accounts are created by HR only. The contract has no registration path.
        SetDefault(context, SelfRegistrationSettingName, "false");
    }

    private static void SetDefault(ISettingDefinitionContext context, string name, string value)
    {
        var definition = context.GetOrNull(name);
        if (definition != null)
        {
            definition.DefaultValue = value;
        }
    }
}
