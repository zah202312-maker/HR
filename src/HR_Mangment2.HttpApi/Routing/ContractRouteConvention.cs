using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace HR_Mangment2.Routing;

/// <summary>
/// Removes every /api route that a framework module opens and the contract does not declare, such as
/// /api/identity, /api/account (self registration), /api/permission-management, /api/multi-tenancy.
/// The modules stay loaded because the sign-in pages depend on them. Non-API routes are not touched.
/// </summary>
public class ContractRouteConvention : IApplicationModelConvention
{
    private const string OwnAssemblyPrefix = "HR_Mangment2";

    private static readonly string[] AllowedFrameworkRoutes =
    {
        "api/abp/application-configuration"
    };

    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            if (controller.ControllerType.Assembly.GetName().Name?.StartsWith(OwnAssemblyPrefix, StringComparison.Ordinal) == true)
            {
                continue;
            }

            foreach (var action in controller.Actions.ToList())
            {
                if (OpensUndeclaredApiRoute(controller, action))
                {
                    controller.Actions.Remove(action);
                }
            }
        }
    }

    private static bool OpensUndeclaredApiRoute(ControllerModel controller, ActionModel action)
    {
        var controllerRoutes = controller.Selectors.Select(s => s.AttributeRouteModel).DefaultIfEmpty(null);

        var templates = action.Selectors
            .SelectMany(selector => controllerRoutes.Select(route =>
                AttributeRouteModel.CombineAttributeRouteModel(route, selector.AttributeRouteModel)?.Template))
            .Where(template => template != null)
            .Select(template => template!.TrimStart('/'))
            .ToList();

        return templates.Any(template =>
            template.StartsWith("api/", StringComparison.OrdinalIgnoreCase) &&
            !AllowedFrameworkRoutes.Contains(template, StringComparer.OrdinalIgnoreCase));
    }
}
