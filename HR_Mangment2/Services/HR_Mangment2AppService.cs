using Volo.Abp.Application.Services;
using HR_Mangment2.Localization;

namespace HR_Mangment2.Services;

/* Inherit your application services from this class. */
public abstract class HR_Mangment2AppService : ApplicationService
{
    protected HR_Mangment2AppService()
    {
        LocalizationResource = typeof(HR_Mangment2Resource);
    }
}