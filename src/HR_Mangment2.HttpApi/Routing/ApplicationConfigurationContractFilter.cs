using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Volo.Abp.AspNetCore.Mvc.ApplicationConfigurations;

namespace HR_Mangment2.Routing;

/// <summary>ABP's defaults differ from the contract: localization resources stay out unless asked for, and a visitor's extraProperties is null.</summary>
public class ApplicationConfigurationContractFilter : IAsyncActionFilter
{
    private const string IncludeLocalizationQuery = "includeLocalizationResources";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.Controller is not AbpApplicationConfigurationController)
        {
            await next();
            return;
        }

        if (!context.HttpContext.Request.Query.ContainsKey(IncludeLocalizationQuery))
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument is ApplicationConfigurationRequestOptions options)
                {
                    options.IncludeLocalizationResources = false;
                }
            }
        }

        var executed = await next();
        if (executed.Result is ObjectResult { Value: ApplicationConfigurationDto dto } && dto.CurrentUser?.IsAuthenticated != true)
        {
            dto.ExtraProperties = null!;
        }
    }
}
