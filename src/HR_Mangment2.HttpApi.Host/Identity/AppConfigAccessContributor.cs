using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc.ApplicationConfigurations;
using Volo.Abp.Data;
using Volo.Abp.Users;

namespace HR_Mangment2.Identity;

/// <summary>AppConfigExtraPropertiesDto: the client routes on startup from these values.</summary>
public class AppConfigAccessContributor : IApplicationConfigurationContributor
{
    public async Task ContributeAsync(ApplicationConfigurationContributorContext context)
    {
        var currentUser = context.ServiceProvider.GetRequiredService<ICurrentUser>();
        if (!currentUser.IsAuthenticated || currentUser.Id == null)
        {
            return;
        }

        var state = await context.ServiceProvider.GetRequiredService<AccountAccessEvaluator>().GetAsync(currentUser.Id.Value);
        var extra = context.ApplicationConfiguration.ExtraProperties;

        extra["accessGate"] = state.Gate;
        extra["mustChangePassword"] = state.MustChangePassword;
        extra["mfaRequired"] = state.MfaRequired;
        extra["mfaEnabled"] = state.MfaEnabled;
    }
}
