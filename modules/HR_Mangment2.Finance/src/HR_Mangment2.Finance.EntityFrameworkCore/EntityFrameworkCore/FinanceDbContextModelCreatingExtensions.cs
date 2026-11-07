using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace HR_Mangment2.Finance.EntityFrameworkCore;

public static class FinanceDbContextModelCreatingExtensions
{
    /// <summary>
    /// Called by the module DbContext and by the host DbContext, which owns the migrations.
    /// Every entity of this module must map to <see cref="FinanceDbProperties.DbSchema"/>.
    /// </summary>
    public static void ConfigureFinance(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        const string schema = FinanceDbProperties.DbSchema;

        builder.Entity<SalaryProfile>(b =>
        {
            b.ToTable("SalaryProfiles", schema);
            b.ConfigureByConvention();
            b.Property(x => x.BaseSalary).HasPrecision(18, 2);
            b.Property(x => x.Currency).IsRequired().HasMaxLength(FinanceConsts.CurrencyLength);
            b.HasMany(x => x.Components).WithOne().HasForeignKey(x => x.SalaryProfileId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.TenantId, x.EmployeeId, x.EffectiveFrom }).IsUnique();
        });

        builder.Entity<SalaryComponent>(b =>
        {
            b.ToTable("SalaryComponents", schema);
            b.ConfigureByConvention();
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.Name).IsRequired().HasMaxLength(FinanceConsts.MaxNameLength);
            b.Property(x => x.Type).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.Amount).HasPrecision(18, 2);
        });

        builder.Entity<Adjustment>(b =>
        {
            b.ToTable("Adjustments", schema);
            b.ConfigureByConvention();
            b.Property(x => x.Period).IsRequired().HasMaxLength(FinanceConsts.PeriodLength);
            b.Property(x => x.Type).HasConversion<string>().HasMaxLength(12);
            b.Property(x => x.Amount).HasPrecision(18, 2);
            b.Property(x => x.Currency).IsRequired().HasMaxLength(FinanceConsts.CurrencyLength);
            b.Property(x => x.Reason).IsRequired().HasMaxLength(FinanceConsts.MaxTextLength);
            b.Property(x => x.Source).HasConversion<string>().HasMaxLength(24);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.AttachmentFileName).HasMaxLength(255);
            b.Property(x => x.AttachmentContentType).HasMaxLength(128);
            b.Property(x => x.AttachmentBlobName).HasMaxLength(128);
            b.HasIndex(x => new { x.Period, x.Status });
            b.HasIndex(x => new { x.EmployeeId, x.Period });
            b.HasIndex(x => x.SourceRequestId);
        });

        builder.Entity<AdjustmentSuggestion>(b =>
        {
            b.ToTable("AdjustmentSuggestions", schema);
            b.ConfigureByConvention();
            b.Property(x => x.Period).IsRequired().HasMaxLength(FinanceConsts.PeriodLength);
            b.Property(x => x.SettlementForPeriod).HasMaxLength(FinanceConsts.PeriodLength);
            b.Property(x => x.Type).HasConversion<string>().HasMaxLength(12);
            b.Property(x => x.Basis).HasConversion<string>().HasMaxLength(32);
            b.Property(x => x.QuantityUnit).HasConversion<string>().HasMaxLength(12);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.Quantity).HasPrecision(18, 2);
            b.Property(x => x.SuggestedAmount).HasPrecision(18, 2);
            b.Property(x => x.Currency).HasMaxLength(FinanceConsts.CurrencyLength);
            b.Property(x => x.ReviewNote).HasMaxLength(FinanceConsts.MaxTextLength);
            b.HasIndex(x => new { x.TenantId, x.EmployeeId, x.Period, x.Basis }).IsUnique().HasFilter("\"SourceCorrectionId\" IS NULL");
            b.HasIndex(x => new { x.TenantId, x.SourceCorrectionId }).IsUnique().HasFilter("\"SourceCorrectionId\" IS NOT NULL");
            b.HasIndex(x => new { x.Period, x.Status });
        });

        builder.Entity<PayrollRun>(b =>
        {
            b.ToTable("PayrollRuns", schema);
            b.ConfigureByConvention();
            b.Property(x => x.Period).IsRequired().HasMaxLength(FinanceConsts.PeriodLength);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(12);
            b.Property(x => x.LastReturnReason).HasMaxLength(FinanceConsts.MaxTextLength);
            b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.PayrollRunId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.TenantId, x.Period }).IsUnique();
        });

        builder.Entity<PayrollItem>(b =>
        {
            b.ToTable("PayrollItems", schema);
            b.ConfigureByConvention();
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.Currency).IsRequired().HasMaxLength(FinanceConsts.CurrencyLength);
            b.Property(x => x.Gross).HasPrecision(18, 2);
            b.Property(x => x.Additions).HasPrecision(18, 2);
            b.Property(x => x.Deductions).HasPrecision(18, 2);
            b.Property(x => x.Net).HasPrecision(18, 2);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PayrollItemId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.PayrollRunId, x.EmployeeId }).IsUnique();
            b.HasIndex(x => x.EmployeeId);
        });

        builder.Entity<PayrollLine>(b =>
        {
            b.ToTable("PayrollLines", schema);
            b.ConfigureByConvention();
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.Kind).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.Label).HasMaxLength(FinanceConsts.MaxTextLength);
            b.Property(x => x.Amount).HasPrecision(18, 2);
        });

        builder.Entity<Expense>(b =>
        {
            b.ToTable("Expenses", schema);
            b.ConfigureByConvention();
            b.Property(x => x.Category).IsRequired().HasMaxLength(FinanceConsts.MaxCategoryLength);
            b.Property(x => x.Amount).HasPrecision(18, 2);
            b.Property(x => x.Currency).IsRequired().HasMaxLength(FinanceConsts.CurrencyLength);
            b.Property(x => x.Description).HasMaxLength(FinanceConsts.MaxTextLength);
            b.Property(x => x.Source).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(12);
            b.Property(x => x.AttachmentFileName).HasMaxLength(255);
            b.Property(x => x.AttachmentContentType).HasMaxLength(128);
            b.Property(x => x.AttachmentBlobName).HasMaxLength(128);
            b.HasIndex(x => new { x.Source, x.Date });
            b.HasIndex(x => x.StockMovementId);
            b.HasIndex(x => x.SourceRequestId);
        });
    }
}
