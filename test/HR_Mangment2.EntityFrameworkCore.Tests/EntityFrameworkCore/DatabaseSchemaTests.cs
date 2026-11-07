using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HR_Mangment2.Complaints;
using HR_Mangment2.Complaints.EntityFrameworkCore;
using HR_Mangment2.Finance;
using HR_Mangment2.Finance.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Xunit;

namespace HR_Mangment2.EntityFrameworkCore;

[Collection(HR_Mangment2TestConsts.CollectionDefinitionName)]
public class DatabaseSchemaTests : HR_Mangment2EntityFrameworkCoreTestBase
{
    [Fact]
    public async Task Model_matches_the_migrations()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<HR_Mangment2DbContext>>().GetDbContextAsync();

            context.Database.HasPendingModelChanges().ShouldBeFalse("Add a migration for the model change.");
            (await context.Database.GetPendingMigrationsAsync()).ShouldBeEmpty();
        });
    }

    [Fact]
    public async Task Runs_on_the_dedicated_test_database()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<HR_Mangment2DbContext>>().GetDbContextAsync();

            context.Database.ProviderName.ShouldBe("Npgsql.EntityFrameworkCore.PostgreSQL");
            context.Database.GetDbConnection().Database.ShouldBe("HR_Mangment2_Test_Ef");
        });
    }

    [Fact]
    public void Sensitive_modules_have_their_own_schema_and_connection_name()
    {
        ComplaintsDbProperties.DbSchema.ShouldBe("complaints");
        FinanceDbProperties.DbSchema.ShouldBe("finance");

        typeof(ComplaintsDbContext).GetCustomAttribute<ConnectionStringNameAttribute>()!.Name.ShouldBe("Complaints");
        typeof(FinanceDbContext).GetCustomAttribute<ConnectionStringNameAttribute>()!.Name.ShouldBe("Finance");
    }
}
