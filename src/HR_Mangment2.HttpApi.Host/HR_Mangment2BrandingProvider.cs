using Microsoft.Extensions.Localization;
using HR_Mangment2.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace HR_Mangment2;

[Dependency(ReplaceServices = true)]
public class HR_Mangment2BrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<HR_Mangment2Resource> _localizer;

    public HR_Mangment2BrandingProvider(IStringLocalizer<HR_Mangment2Resource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
