using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace HR_Mangment2.Finance.EntityFrameworkCore;

[ConnectionStringName(FinanceDbProperties.ConnectionStringName)]
public class FinanceDbContext : AbpDbContext<FinanceDbContext>
{
    public FinanceDbContext(DbContextOptions<FinanceDbContext> options)
        : base(options)
    {
    }

    public DbSet<SalaryProfile> SalaryProfiles { get; set; }
    public DbSet<SalaryComponent> SalaryComponents { get; set; }
    public DbSet<Adjustment> Adjustments { get; set; }
    public DbSet<AdjustmentSuggestion> AdjustmentSuggestions { get; set; }
    public DbSet<PayrollRun> PayrollRuns { get; set; }
    public DbSet<PayrollItem> PayrollItems { get; set; }
    public DbSet<PayrollLine> PayrollLines { get; set; }
    public DbSet<Expense> Expenses { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureFinance();
    }
}
