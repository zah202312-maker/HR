using Volo.Abp.Modularity;

namespace HR_Mangment2;

/* Inherit from this class for your domain layer tests. */
public abstract class HR_Mangment2DomainTestBase<TStartupModule> : HR_Mangment2TestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
