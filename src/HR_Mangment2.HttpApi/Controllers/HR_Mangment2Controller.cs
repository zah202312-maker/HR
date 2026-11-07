using HR_Mangment2.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace HR_Mangment2.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class HR_Mangment2Controller : AbpControllerBase
{
    protected HR_Mangment2Controller()
    {
        LocalizationResource = typeof(HR_Mangment2Resource);
    }
}
