using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using HR_Mangment2.Localization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Account.Web.Pages.Account;
using Volo.Abp.Identity;
using Volo.Abp.Identity.AspNetCore;

namespace HR_Mangment2.Pages.Account;

/// <summary>Sign-in step for accounts with two-step verification: a TOTP code or a one-time recovery code.</summary>
public class LoginTotpModel : AccountPageModel
{
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrlHash { get; set; }

    [BindProperty, Required, StringLength(32)]
    public string Code { get; set; } = string.Empty;

    public bool IsInvalid { get; private set; }

    public LoginTotpModel()
    {
        LocalizationResourceType = typeof(HR_Mangment2Resource);
    }

    public async Task<IActionResult> OnGetAsync()
    {
        return await SignInManager.GetTwoFactorAuthenticationUserAsync() == null
            ? RedirectToPage("./Login", new { returnUrl = ReturnUrl })
            : Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await SignInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            return RedirectToPage("./Login", new { returnUrl = ReturnUrl });
        }

        var code = (Code ?? string.Empty).Trim();
        var result = code.Length == 6 && long.TryParse(code, out _)
            ? await SignInManager.TwoFactorAuthenticatorSignInAsync(code, isPersistent: false, rememberClient: false)
            : await SignInManager.TwoFactorRecoveryCodeSignInAsync(code);

        await IdentitySecurityLogManager.SaveAsync(new IdentitySecurityLogContext
        {
            Identity = IdentitySecurityLogIdentityConsts.Identity,
            Action = result.Succeeded ? IdentitySecurityLogActionConsts.LoginSucceeded : IdentitySecurityLogActionConsts.LoginFailed,
            UserName = user.UserName
        });

        if (result.Succeeded)
        {
            return await RedirectSafelyAsync(ReturnUrl ?? "~/", ReturnUrlHash);
        }

        IsInvalid = true;
        return Page();
    }
}
