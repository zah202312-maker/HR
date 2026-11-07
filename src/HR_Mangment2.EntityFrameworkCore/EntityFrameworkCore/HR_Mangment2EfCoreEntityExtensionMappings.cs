using System;
using HR_Mangment2.Identity;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Threading;

namespace HR_Mangment2.EntityFrameworkCore;

public static class HR_Mangment2EfCoreEntityExtensionMappings
{
    private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public static void Configure()
    {
        HR_Mangment2GlobalFeatureConfigurator.Configure();
        HR_Mangment2ModuleExtensionConfigurator.Configure();

        OneTimeRunner.Run(() =>
        {
            ObjectExtensionManager.Instance
                .MapEfCoreProperty<IdentityUser, bool>(HrUserProperties.MustChangePassword)
                .MapEfCoreProperty<IdentityUser, string>(
                    HrUserProperties.PreferredLanguage,
                    (_, property) => property.HasMaxLength(8))
                .MapEfCoreProperty<IdentityUser, DateTime?>(HrUserProperties.MfaEnabledAt);
        });
    }
}
