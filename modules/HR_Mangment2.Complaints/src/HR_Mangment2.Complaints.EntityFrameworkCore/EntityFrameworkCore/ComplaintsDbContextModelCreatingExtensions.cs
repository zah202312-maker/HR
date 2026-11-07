using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace HR_Mangment2.Complaints.EntityFrameworkCore;

public static class ComplaintsDbContextModelCreatingExtensions
{
    /// <summary>
    /// Called by the module DbContext and by the host DbContext, which owns the migrations.
    /// Every entity of this module must map to <see cref="ComplaintsDbProperties.DbSchema"/>.
    /// People are kept as plain ids with no foreign key, so the restricted account never touches other schemas.
    /// </summary>
    public static void ConfigureComplaints(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Complaint>(b =>
        {
            b.ToTable("Complaints", ComplaintsDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ReferenceNumber).IsRequired().HasMaxLength(Complaint.ReferenceNumberLength);
            b.Property(x => x.TargetKind).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.RespondentCategory).HasConversion<string>().HasMaxLength(24);
            b.Property(x => x.Level).HasConversion<int>();
            b.Property(x => x.MinimumLevel).HasConversion<int>();
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            b.Property(x => x.WrappedKey).IsRequired();
            b.Property(x => x.EncryptedSubject).IsRequired();
            b.Property(x => x.EncryptedContent).IsRequired();
            b.Ignore(x => x.IsClosed);
            b.HasMany(x => x.Actions).WithOne().HasForeignKey(x => x.ComplaintId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Attachments).WithOne().HasForeignKey(x => x.ComplaintId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.TenantId, x.ReferenceNumber }).IsUnique();
            b.HasIndex(x => new { x.ComplainantUserId, x.SubmittedAt });
            b.HasIndex(x => new { x.DepartmentManagerUserId, x.Status });
            b.HasIndex(x => new { x.Level, x.Status, x.SubmittedAt });
            b.HasIndex(x => new { x.Status, x.DueAt });
            b.HasIndex(x => x.ComplainantEmployeeId);
        });

        builder.Entity<ComplaintAction>(b =>
        {
            b.ToTable("ComplaintActions", ComplaintsDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.Type).HasConversion<string>().HasMaxLength(24);
            b.HasIndex(x => new { x.ComplaintId, x.CreationTime });
        });

        builder.Entity<ComplaintAttachment>(b =>
        {
            b.ToTable("ComplaintAttachments", ComplaintsDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.EncryptedFileName).IsRequired();
            b.Property(x => x.ContentType).IsRequired().HasMaxLength(ComplaintAttachment.MaxContentTypeLength);
            b.Property(x => x.BlobName).IsRequired().HasMaxLength(64);
            b.HasIndex(x => x.ComplaintId);
        });

        builder.Entity<ComplaintAccessLog>(b =>
        {
            b.ToTable("ComplaintAccessLogs", ComplaintsDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Action).HasConversion<string>().HasMaxLength(24);
            b.HasIndex(x => new { x.ComplaintId, x.At });
        });

        builder.Entity<ComplaintHandlerAssignment>(b =>
        {
            b.ToTable("ComplaintHandlerAssignments", ComplaintsDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Level).HasConversion<int>();
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
            b.Property(x => x.Reason).IsRequired().HasMaxLength(ComplaintHandlerAssignment.MaxReasonLength);
            b.Property(x => x.DecisionReason).HasMaxLength(ComplaintHandlerAssignment.MaxReasonLength);
            b.HasIndex(x => new { x.UserId, x.Status });
            b.HasIndex(x => new { x.Status, x.To });
        });
    }
}
