using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Volo.Abp.Account.Web;
using Volo.Abp.Account.Web.Pages.Account;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.OpenIddict;

namespace HR_Mangment2.Pages.Account;

/// <summary>The framework page stops at a two-step account, so the code step continues on our page.</summary>
[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(LoginModel), typeof(OpenIddictSupportedLoginModel))]
public class HrLoginModel : OpenIddictSupportedLoginModel
{
    public HrLoginModel(
        IAuthenticationSchemeProvider schemeProvider,
        IOptions<AbpAccountOptions> accountOptions,
        IOptions<IdentityOptions> identityOptions,
        IdentityDynamicClaimsPrincipalContributorCache identityDynamicClaimsPrincipalContributorCache,
        AbpOpenIddictRequestHelper openIddictRequestHelper,
        Microsoft.AspNetCore.Hosting.IWebHostEnvironment webHostEnvironment)
        : base(schemeProvider, accountOptions, identityOptions, identityDynamicClaimsPrincipalContributorCache, openIddictRequestHelper, webHostEnvironment)
    {
    }

    protected override Task<IActionResult> TwoFactorLoginResultAsync()
    {
        return Task.FromResult<IActionResult>(RedirectToPage("./LoginTotp", new { returnUrl = ReturnUrl, returnUrlHash = ReturnUrlHash }));
    }
}
