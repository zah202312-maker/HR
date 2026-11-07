using Volo.Abp.Settings;

namespace HR_Mangment2.Settings;

public class HR_Mangment2SettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(HR_Mangment2Settings.DocumentMaxFileSizeBytes, "10485760", isVisibleToClients: true),
            new SettingDefinition(HR_Mangment2Settings.DocumentAllowedContentTypes, "application/pdf,image/jpeg,image/png", isVisibleToClients: true));
    }
}
