namespace HR_Mangment2.Finance;

/// <summary>
/// Database placement of the Finance module. Tables live in their own schema, and the runtime
/// connection string can point to a restricted database account without touching the code.
/// Migrations stay unified in the host DbContext, which runs with the owner account.
/// </summary>
public static class FinanceDbProperties
{
    public const string DbSchema = "finance";

    public const string ConnectionStringName = "Finance";
}
