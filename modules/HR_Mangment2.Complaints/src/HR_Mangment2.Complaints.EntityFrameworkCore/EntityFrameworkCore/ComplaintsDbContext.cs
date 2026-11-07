using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace HR_Mangment2.Complaints.EntityFrameworkCore;

[ConnectionStringName(ComplaintsDbProperties.ConnectionStringName)]
public class ComplaintsDbContext : AbpDbContext<ComplaintsDbContext>
{
    public DbSet<Complaint> Complaints { get; set; }
    public DbSet<ComplaintAction> ComplaintActions { get; set; }
    public DbSet<ComplaintAttachment> ComplaintAttachments { get; set; }
    public DbSet<ComplaintAccessLog> ComplaintAccessLogs { get; set; }
    public DbSet<ComplaintHandlerAssignment> ComplaintHandlerAssignments { get; set; }

    public ComplaintsDbContext(DbContextOptions<ComplaintsDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.AddInterceptors(new ComplaintsTenantSessionInterceptor());
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureComplaints();
    }
}

/// <summary>
/// Row-level security reads the company from this session setting. Under the restricted account, a connection that
/// did not set it sees no rows. The owner account used by migrations is not subject to the policies.
/// </summary>
public class ComplaintsTenantSessionInterceptor : DbConnectionInterceptor
{
    public const string SettingName = "hr.tenant_id";

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        using var command = CreateCommand(connection, eventData);
        command.ExecuteNonQuery();
    }

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await using var command = CreateCommand(connection, eventData);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static DbCommand CreateCommand(DbConnection connection, ConnectionEndEventData eventData)
    {
        var tenantId = (eventData.Context as ComplaintsDbContext)?.CurrentTenant.Id;

        var command = connection.CreateCommand();
        command.CommandText = "SELECT set_config('" + SettingName + "', @tenant, false)";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "tenant";
        parameter.Value = tenantId?.ToString() ?? string.Empty;
        command.Parameters.Add(parameter);

        return command;
    }
}
