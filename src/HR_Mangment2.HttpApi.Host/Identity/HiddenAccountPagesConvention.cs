using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace HR_Mangment2.Identity;

/// <summary>
/// Removes framework account pages the system must not offer: accounts come from HR only,
/// and a password reset starts from HR or the executive, not from the user.
/// </summary>
public class HiddenAccountPagesConvention : IPageRouteModelConvention
{
    private static readonly string[] Hidden =
    {
        "/Account/Register", "/Account/ForgotPassword", "/Account/PasswordResetLinkSent", "/Account/Manage", "/Account/SecurityLogs"
    };

    public void Apply(PageRouteModel model)
    {
        if (Hidden.Contains(model.ViewEnginePath, StringComparer.OrdinalIgnoreCase))
        {
            model.Selectors.Clear();
        }
    }
}
