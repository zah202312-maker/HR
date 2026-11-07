namespace HR_Mangment2.Complaints;

/// <summary>
/// Database placement of the Complaints module. Tables live in their own schema, and the runtime
/// connection string can point to a restricted database account without touching the code.
/// Migrations stay unified in the host DbContext, which runs with the owner account.
/// </summary>
public static class ComplaintsDbProperties
{
    public const string DbSchema = "complaints";

    public const string ConnectionStringName = "Complaints";
}
