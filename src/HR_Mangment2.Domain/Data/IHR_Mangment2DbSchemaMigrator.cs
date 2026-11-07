using System.Threading.Tasks;

namespace HR_Mangment2.Data;

public interface IHR_Mangment2DbSchemaMigrator
{
    Task MigrateAsync();
}
