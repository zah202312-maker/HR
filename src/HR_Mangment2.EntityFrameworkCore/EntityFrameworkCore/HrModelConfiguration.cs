using System;
using HR_Mangment2.Attendance;
using HR_Mangment2.Auditing;
using HR_Mangment2.Delegations;
using HR_Mangment2.Leaves;
using HR_Mangment2.Requests;
using HR_Mangment2.Departments;
using HR_Mangment2.Employees;
using HR_Mangment2.Files;
using HR_Mangment2.Holidays;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Notifications;
using HR_Mangment2.Settings;
using HR_Mangment2.WorkShifts;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace HR_Mangment2.EntityFrameworkCore;

public static class HrModelConfiguration
{
    private const string Prefix = "App";

    private static void ConfigureEmploymentRecords(ModelBuilder builder)
    {
        builder.Entity<EmployeeHireDateCorrection>(b =>
        {
            b.ToTable(Prefix + "EmployeeHireDateCorrections");
            b.ConfigureByConvention();
            b.Property(x => x.Reason).IsRequired().HasMaxLength(EmployeeHireDateCorrection.MaxReasonLength);
            b.HasOne<Employee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.EmployeeId);
        });

        builder.Entity<HR_Mangment2.EmploymentContracts.EmploymentContract>(b =>
        {
            b.ToTable(Prefix + "Contracts");
            b.ConfigureByConvention();
            b.Property(x => x.ContractType).IsRequired().HasMaxLength(HR_Mangment2.EmploymentContracts.EmploymentContract.MaxContractTypeLength);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
            b.Property(x => x.Recommendation).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.Decision).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.EmployeePosition).HasMaxLength(HR_Mangment2.EmploymentContracts.EmploymentContract.MaxTextLength);
            b.Property(x => x.RecommendationNote).HasMaxLength(HR_Mangment2.EmploymentContracts.EmploymentContract.MaxTextLength);
            b.Property(x => x.DecisionNote).HasMaxLength(HR_Mangment2.EmploymentContracts.EmploymentContract.MaxTextLength);
            b.Property(x => x.EmployeeMessage).HasMaxLength(HR_Mangment2.EmploymentContracts.EmploymentContract.MaxTextLength);
            b.HasOne<Employee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<HR_Mangment2.EmploymentContracts.EmploymentContract>().WithMany().HasForeignKey(x => x.PreviousContractId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.EmployeeId, x.StartDate });
            b.HasIndex(x => new { x.Status, x.EndDate });
        });

        builder.Entity<HR_Mangment2.Warnings.Warning>(b =>
        {
            b.ToTable(Prefix + "Warnings");
            b.ConfigureByConvention();
            b.Property(x => x.RecommendedLevel).HasConversion<string>().HasMaxLength(8);
            b.Property(x => x.IssuedLevel).HasConversion<string>().HasMaxLength(8);
            b.Property(x => x.RecommenderRole).HasConversion<string>().HasMaxLength(24);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
            b.Property(x => x.DecisionOutcome).HasConversion<string>().HasMaxLength(12);
            b.Property(x => x.ObjectionOutcome).HasConversion<string>().HasMaxLength(12);
            b.Property(x => x.Description).IsRequired().HasMaxLength(HR_Mangment2.Warnings.Warning.MaxDescriptionLength);
            b.Property(x => x.ResponseContent).HasMaxLength(HR_Mangment2.Warnings.Warning.MaxDescriptionLength);
            b.Property(x => x.ObjectionContent).HasMaxLength(HR_Mangment2.Warnings.Warning.MaxDescriptionLength);
            b.Property(x => x.DecisionNote).HasMaxLength(HR_Mangment2.Warnings.Warning.MaxNoteLength);
            b.Property(x => x.ObjectionResolutionNote).HasMaxLength(HR_Mangment2.Warnings.Warning.MaxNoteLength);
            b.HasOne<Employee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Attachments).WithOne().HasForeignKey(x => x.WarningId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.EmployeeId, x.Status });
            b.HasIndex(x => new { x.RecommendedByUserId, x.CreationTime });
            b.HasIndex(x => new { x.Status, x.ResponseDueDate });
        });

        builder.Entity<HR_Mangment2.Warnings.WarningAttachment>(b =>
        {
            b.ToTable(Prefix + "WarningAttachments");
            b.ConfigureByConvention();
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.FileName).IsRequired().HasMaxLength(FileDownloadLink.MaxFileNameLength);
            b.Property(x => x.ContentType).IsRequired().HasMaxLength(FileDownloadLink.MaxContentTypeLength);
            b.Property(x => x.BlobName).IsRequired().HasMaxLength(FileDownloadLink.MaxBlobNameLength);
        });

        builder.Entity<HR_Mangment2.Appreciation.AppreciationLetter>(b =>
        {
            b.ToTable(Prefix + "AppreciationLetters");
            b.ConfigureByConvention();
            b.Property(x => x.Title).IsRequired().HasMaxLength(HR_Mangment2.Appreciation.AppreciationLetter.MaxTitleLength);
            b.Property(x => x.Note).HasMaxLength(HR_Mangment2.Appreciation.AppreciationLetter.MaxNoteLength);
            b.Property(x => x.FileName).IsRequired().HasMaxLength(FileDownloadLink.MaxFileNameLength);
            b.Property(x => x.ContentType).IsRequired().HasMaxLength(FileDownloadLink.MaxContentTypeLength);
            b.Property(x => x.BlobName).IsRequired().HasMaxLength(FileDownloadLink.MaxBlobNameLength);
            b.HasOne<Employee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.EmployeeId, x.IssuedAt });
        });
    }

    private static void ConfigureEvaluations(ModelBuilder builder)
    {
        builder.Entity<HR_Mangment2.Evaluations.EvaluationCriterion>(b =>
        {
            b.ToTable(Prefix + "EvaluationCriteria");
            b.ConfigureByConvention();
            b.Property(x => x.NameAr).IsRequired().HasMaxLength(HR_Mangment2.Evaluations.EvaluationConsts.MaxCriterionNameLength);
            b.Property(x => x.NameEn).IsRequired().HasMaxLength(HR_Mangment2.Evaluations.EvaluationConsts.MaxCriterionNameLength);
            b.HasIndex(x => new { x.TenantId, x.NameAr }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.NameEn }).IsUnique();
        });

        builder.Entity<HR_Mangment2.Evaluations.EvaluationGrade>(b =>
        {
            b.ToTable(Prefix + "EvaluationGrades");
            b.ConfigureByConvention();
            b.Property(x => x.Code).IsRequired().HasMaxLength(HR_Mangment2.Evaluations.EvaluationConsts.MaxGradeCodeLength);
            b.Property(x => x.NameAr).IsRequired().HasMaxLength(HR_Mangment2.Evaluations.EvaluationConsts.MaxGradeNameLength);
            b.Property(x => x.NameEn).IsRequired().HasMaxLength(HR_Mangment2.Evaluations.EvaluationConsts.MaxGradeNameLength);
            b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        });

        builder.Entity<HR_Mangment2.Evaluations.EvaluationCycle>(b =>
        {
            b.ToTable(Prefix + "EvaluationCycles");
            b.ConfigureByConvention();
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
            b.Ignore(x => x.PeriodFrom);
            b.Ignore(x => x.PeriodTo);
            b.HasMany(x => x.Criteria).WithOne().HasForeignKey(x => x.CycleId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Grades).WithOne().HasForeignKey(x => x.CycleId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.TenantId, x.Year, x.Month }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.Status });
        });

        builder.Entity<HR_Mangment2.Evaluations.EvaluationCycleCriterion>(b =>
        {
            b.ToTable(Prefix + "EvaluationCycleCriteria");
            b.ConfigureByConvention();
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.NameAr).IsRequired().HasMaxLength(HR_Mangment2.Evaluations.EvaluationConsts.MaxCriterionNameLength);
            b.Property(x => x.NameEn).IsRequired().HasMaxLength(HR_Mangment2.Evaluations.EvaluationConsts.MaxCriterionNameLength);
            b.HasIndex(x => new { x.CycleId, x.CriterionId }).IsUnique();
        });

        builder.Entity<HR_Mangment2.Evaluations.EvaluationCycleGrade>(b =>
        {
            b.ToTable(Prefix + "EvaluationCycleGrades");
            b.ConfigureByConvention();
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.Code).IsRequired().HasMaxLength(HR_Mangment2.Evaluations.EvaluationConsts.MaxGradeCodeLength);
            b.Property(x => x.NameAr).IsRequired().HasMaxLength(HR_Mangment2.Evaluations.EvaluationConsts.MaxGradeNameLength);
            b.Property(x => x.NameEn).IsRequired().HasMaxLength(HR_Mangment2.Evaluations.EvaluationConsts.MaxGradeNameLength);
            b.HasIndex(x => new { x.CycleId, x.Code }).IsUnique();
        });

        builder.Entity<HR_Mangment2.Evaluations.Evaluation>(b =>
        {
            b.ToTable(Prefix + "Evaluations");
            b.ConfigureByConvention();
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.ObjectionStatus).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.EmployeeNotes).HasMaxLength(HR_Mangment2.Evaluations.EvaluationConsts.MaxNotesLength);
            b.Property(x => x.LeaveDays).HasPrecision(18, 4);
            b.HasMany(x => x.Scores).WithOne().HasForeignKey(x => x.EvaluationId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Responses).WithOne().HasForeignKey(x => x.EvaluationId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<HR_Mangment2.Evaluations.EvaluationCycle>().WithMany().HasForeignKey(x => x.CycleId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Employee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.CycleId, x.EmployeeId }).IsUnique();
            b.HasIndex(x => new { x.EmployeeId, x.Status });
            b.HasIndex(x => new { x.CycleId, x.Status });
            b.HasIndex(x => x.ObjectionStatus);
        });

        builder.Entity<HR_Mangment2.Evaluations.EvaluationScore>(b =>
        {
            b.ToTable(Prefix + "EvaluationScores");
            b.ConfigureByConvention();
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.GradeCode).IsRequired().HasMaxLength(HR_Mangment2.Evaluations.EvaluationConsts.MaxGradeCodeLength);
            b.HasIndex(x => new { x.EvaluationId, x.CriterionId }).IsUnique();
        });

        builder.Entity<HR_Mangment2.Evaluations.EvaluationResponse>(b =>
        {
            b.ToTable(Prefix + "EvaluationResponses");
            b.ConfigureByConvention();
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.Type).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.Decision).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.Content).IsRequired().HasMaxLength(HR_Mangment2.Evaluations.EvaluationConsts.MaxResponseLength);
            b.Property(x => x.DecisionReason).HasMaxLength(HR_Mangment2.Evaluations.EvaluationConsts.MaxReasonLength);
            b.HasIndex(x => new { x.EvaluationId, x.CreationTime });
        });

        builder.Entity<HR_Mangment2.Evaluations.EvaluationInternalNote>(b =>
        {
            b.ToTable(Prefix + "EvaluationInternalNotes");
            b.ConfigureByConvention();
            b.Property(x => x.Content).IsRequired().HasMaxLength(HR_Mangment2.Evaluations.EvaluationConsts.MaxInternalNoteLength);
            b.HasOne<HR_Mangment2.Evaluations.Evaluation>().WithMany().HasForeignKey(x => x.EvaluationId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.EvaluationId, x.CreationTime });
            b.HasIndex(x => new { x.EvaluationId, x.AuthorUserId });
        });

        builder.Entity<HR_Mangment2.Evaluations.EvaluationInternalNoteDraft>(b =>
        {
            b.ToTable(Prefix + "EvaluationInternalNoteDrafts");
            b.ConfigureByConvention();
            b.Property(x => x.Content).IsRequired().HasMaxLength(HR_Mangment2.Evaluations.EvaluationConsts.MaxInternalNoteLength);
            b.HasOne<HR_Mangment2.Evaluations.Evaluation>().WithMany().HasForeignKey(x => x.EvaluationId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.EvaluationId, x.AuthorUserId }).IsUnique();
        });

        builder.Entity<HR_Mangment2.Evaluations.DepartmentReport>(b =>
        {
            b.ToTable(Prefix + "DepartmentReports");
            b.ConfigureByConvention();
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.SnapshotJson).IsRequired();
            b.Property(x => x.Notes).HasMaxLength(HR_Mangment2.Evaluations.EvaluationConsts.MaxNotesLength);
            b.HasOne<HR_Mangment2.Evaluations.EvaluationCycle>().WithMany().HasForeignKey(x => x.CycleId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.CycleId, x.ManagerUserId }).IsUnique();
            b.HasIndex(x => x.ManagerUserId);
            b.HasIndex(x => x.DepartmentId);
        });

        builder.Entity<HR_Mangment2.Evaluations.Circular>(b =>
        {
            b.ToTable(Prefix + "Circulars");
            b.ConfigureByConvention();
            b.Property(x => x.Scope).HasConversion<string>().HasMaxLength(24);
            b.Property(x => x.Title).IsRequired().HasMaxLength(HR_Mangment2.Evaluations.Circular.MaxTitleLength);
            b.Property(x => x.Content).IsRequired().HasMaxLength(HR_Mangment2.Evaluations.Circular.MaxContentLength);
            b.HasMany(x => x.Recipients).WithOne().HasForeignKey(x => x.CircularId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.SenderUserId, x.SentAt });
        });

        builder.Entity<HR_Mangment2.Evaluations.CircularRecipient>(b =>
        {
            b.ToTable(Prefix + "CircularRecipients");
            b.ConfigureByConvention();
            b.Property(x => x.Id).ValueGeneratedNever();
            b.HasOne<Employee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.CircularId, x.UserId }).IsUnique();
            b.HasIndex(x => new { x.UserId, x.AcknowledgedAt });
        });
    }

    private static void ConfigureStore(ModelBuilder builder)
    {
        builder.Entity<HR_Mangment2.Store.Item>(b =>
        {
            b.ToTable(Prefix + "Items");
            b.ConfigureByConvention();
            b.Property(x => x.NameAr).IsRequired().HasMaxLength(HR_Mangment2.Store.StoreConsts.MaxNameLength);
            b.Property(x => x.NameEn).IsRequired().HasMaxLength(HR_Mangment2.Store.StoreConsts.MaxNameLength);
            b.Property(x => x.Kind).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.MinQuantity).HasPrecision(HR_Mangment2.Store.StoreConsts.QuantityPrecision, HR_Mangment2.Store.StoreConsts.QuantityScale);
            b.Property(x => x.QuantityOnHand).HasPrecision(HR_Mangment2.Store.StoreConsts.QuantityPrecision, HR_Mangment2.Store.StoreConsts.QuantityScale);
            b.Ignore(x => x.IsBelowMinimum);
            b.HasIndex(x => new { x.TenantId, x.NameAr }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.NameEn }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.IsActive });
        });

        builder.Entity<HR_Mangment2.Store.StockMovement>(b =>
        {
            b.ToTable(Prefix + "StockMovements");
            b.ConfigureByConvention();
            b.Property(x => x.Type).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.Quantity).HasPrecision(HR_Mangment2.Store.StoreConsts.QuantityPrecision, HR_Mangment2.Store.StoreConsts.QuantityScale);
            b.Property(x => x.BalanceAfter).HasPrecision(HR_Mangment2.Store.StoreConsts.QuantityPrecision, HR_Mangment2.Store.StoreConsts.QuantityScale);
            b.Property(x => x.TotalCost).HasPrecision(HR_Mangment2.Store.StoreConsts.CostPrecision, HR_Mangment2.Store.StoreConsts.CostScale);
            b.Property(x => x.Note).HasMaxLength(HR_Mangment2.Store.StoreConsts.MaxNoteLength);
            b.HasOne<HR_Mangment2.Store.Item>().WithMany().HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.ItemId, x.CreationTime });
            b.HasIndex(x => new { x.TenantId, x.Type, x.CreationTime });
            b.HasIndex(x => x.ReversesMovementId).IsUnique().HasFilter("\"ReversesMovementId\" IS NOT NULL");
        });

        builder.Entity<HR_Mangment2.Store.StockIssue>(b =>
        {
            b.ToTable(Prefix + "StockIssues");
            b.ConfigureByConvention();
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
            b.Property(x => x.Quantity).HasPrecision(HR_Mangment2.Store.StoreConsts.QuantityPrecision, HR_Mangment2.Store.StoreConsts.QuantityScale);
            b.HasOne<HR_Mangment2.Store.Item>().WithMany().HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Employee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Request>().WithMany().HasForeignKey(x => x.RequestId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.RequestId).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.Status, x.CreationTime });
            b.HasIndex(x => x.EmployeeId);
        });

        builder.Entity<HR_Mangment2.Store.Custody>(b =>
        {
            b.ToTable(Prefix + "Custodies");
            b.ConfigureByConvention();
            b.Property(x => x.Kind).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
            b.Property(x => x.Quantity).HasPrecision(HR_Mangment2.Store.StoreConsts.QuantityPrecision, HR_Mangment2.Store.StoreConsts.QuantityScale);
            b.Property(x => x.Note).HasMaxLength(HR_Mangment2.Store.StoreConsts.MaxNoteLength);
            b.Property(x => x.ReturnNote).HasMaxLength(HR_Mangment2.Store.StoreConsts.MaxNoteLength);
            b.HasOne<HR_Mangment2.Store.Item>().WithMany().HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Employee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Department>().WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.EmployeeId, x.Status });
            b.HasIndex(x => new { x.DepartmentId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.Status, x.AssignedAt });
            b.HasIndex(x => x.ItemId);
        });

        builder.Entity<HR_Mangment2.Store.CustodyCount>(b =>
        {
            b.ToTable(Prefix + "CustodyCounts");
            b.ConfigureByConvention();
            b.Property(x => x.Result).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.Note).HasMaxLength(HR_Mangment2.Store.StoreConsts.MaxNoteLength);
            b.HasOne<Department>().WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.CustodyCountId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.DepartmentId, x.CountDate });
            b.HasIndex(x => new { x.TenantId, x.CountDate });
        });

        builder.Entity<HR_Mangment2.Store.CustodyCountLine>(b =>
        {
            b.ToTable(Prefix + "CustodyCountLines");
            b.ConfigureByConvention();
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.ExpectedQuantity).HasPrecision(HR_Mangment2.Store.StoreConsts.QuantityPrecision, HR_Mangment2.Store.StoreConsts.QuantityScale);
            b.Property(x => x.CountedQuantity).HasPrecision(HR_Mangment2.Store.StoreConsts.QuantityPrecision, HR_Mangment2.Store.StoreConsts.QuantityScale);
            b.Property(x => x.Note).HasMaxLength(HR_Mangment2.Store.StoreConsts.MaxNoteLength);
            b.HasIndex(x => x.CustodyCountId);
            b.HasIndex(x => x.CustodyId);
        });
    }

    private static void ConfigureReviewReports(ModelBuilder builder)
    {
        builder.Entity<HR_Mangment2.ReviewReports.ReviewReport>(b =>
        {
            b.ToTable(Prefix + "ReviewReports");
            b.ConfigureByConvention();
            b.Property(x => x.PeriodType).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.Summary).HasMaxLength(HR_Mangment2.ReviewReports.ReviewReportConsts.MaxSummaryLength);
            b.HasMany(x => x.Teams).WithOne().HasForeignKey(x => x.ReviewReportId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.ReviewerUserId, x.PeriodFrom });
            b.HasIndex(x => new { x.Status, x.SubmittedAt });
        });

        builder.Entity<HR_Mangment2.ReviewReports.ReviewReportTeam>(b =>
        {
            b.ToTable(Prefix + "ReviewReportTeams");
            b.ConfigureByConvention();
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.Notes).HasMaxLength(HR_Mangment2.ReviewReports.ReviewReportConsts.MaxNotesLength);
            b.Property(x => x.ProjectNotes).HasMaxLength(HR_Mangment2.ReviewReports.ReviewReportConsts.MaxNotesLength);
            b.HasOne<Employee>().WithMany().HasForeignKey(x => x.ManagerEmployeeId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.ReviewReportId, x.ManagerEmployeeId }).IsUnique();
        });
    }

    /// <summary>Tables of the main business schema. Each module of the system adds its mapping here.</summary>
    public static void ConfigureHr(this ModelBuilder builder)
    {
        ConfigureStore(builder);
        ConfigureEvaluations(builder);
        ConfigureEmploymentRecords(builder);
        ConfigureReviewReports(builder);

        builder.Entity<Notification>(b =>
        {
            b.ToTable(Prefix + "Notifications");
            b.ConfigureByConvention();
            b.Property(x => x.TemplateCode).HasConversion<string>().HasMaxLength(64);
            b.Property(x => x.ResourceType).HasConversion<string>().HasMaxLength(32);
            b.HasIndex(x => new { x.UserId, x.ReadAt });
        });

        builder.Entity<DeviceToken>(b =>
        {
            b.ToTable(Prefix + "DeviceTokens");
            b.ConfigureByConvention();
            b.Property(x => x.Token).IsRequired().HasMaxLength(DeviceToken.MaxTokenLength);
            b.Property(x => x.Platform).HasConversion<string>().HasMaxLength(16);
            b.HasIndex(x => x.Token).IsUnique();
            b.HasIndex(x => x.UserId);
        });

        builder.Entity<HrAuditEntry>(b =>
        {
            b.ToTable(Prefix + "AuditEntries");
            b.ConfigureByConvention();
            b.Property(x => x.Action).HasConversion<string>().HasMaxLength(32);
            b.Property(x => x.EntityType).HasMaxLength(128);
            b.Property(x => x.EntityId).HasMaxLength(64);
            b.Property(x => x.IpAddress).HasMaxLength(64);
            b.HasIndex(x => new { x.TenantId, x.OccurredAt });
            b.HasIndex(x => new { x.TenantId, x.UserId, x.OccurredAt });
            b.HasIndex(x => new { x.TenantId, x.Action, x.OccurredAt });
            b.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId });
        });

        builder.Entity<SystemSettingSection>(b =>
        {
            b.ToTable(Prefix + "SystemSettingSections");
            b.ConfigureByConvention();
            b.Property(x => x.Section).IsRequired().HasMaxLength(64);
            b.Property(x => x.ValueJson).IsRequired().HasColumnType("jsonb");
            b.HasIndex(x => new { x.TenantId, x.Section }).IsUnique();
        });

        builder.Entity<IdempotencyRecord>(b =>
        {
            b.ToTable(Prefix + "IdempotencyRecords");
            b.ConfigureByConvention();
            b.Property(x => x.Operation).IsRequired().HasMaxLength(IdempotencyRecord.MaxOperationLength);
            b.Property(x => x.RequestHash).IsRequired().HasMaxLength(IdempotencyRecord.RequestHashLength);
            b.HasIndex(x => new { x.TenantId, x.UserId, x.Operation, x.Key }).IsUnique();
            b.HasIndex(x => x.ExpiresAt);
        });

        builder.Entity<Department>(b =>
        {
            b.ToTable(Prefix + "Departments");
            b.ConfigureByConvention();
            b.Property(x => x.NameAr).IsRequired().HasMaxLength(Department.MaxNameLength);
            b.Property(x => x.NameEn).IsRequired().HasMaxLength(Department.MaxNameLength);
            b.HasIndex(x => x.NameAr);
            b.HasIndex(x => x.NameEn);
        });

        builder.Entity<WorkShift>(b =>
        {
            b.ToTable(Prefix + "WorkShifts");
            b.ConfigureByConvention();
            b.Property(x => x.NameAr).IsRequired().HasMaxLength(WorkShift.MaxNameLength);
            b.Property(x => x.NameEn).IsRequired().HasMaxLength(WorkShift.MaxNameLength);
            b.Property(x => x.Type).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.OvertimePolicy).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.OvertimeAfterHours).HasPrecision(6, 2);
            b.OwnsMany(x => x.Days, day =>
            {
                day.ToTable(Prefix + "WorkShiftDays");
                day.WithOwner().HasForeignKey("WorkShiftId");
                day.Property<Guid>("Id");
                day.HasKey("Id");
                day.Property(x => x.DayOfWeek).HasConversion<string>().HasMaxLength(16);
            });
            b.HasIndex(x => new { x.Type, x.IsActive });
        });

        builder.Entity<Holiday>(b =>
        {
            b.ToTable(Prefix + "Holidays");
            b.ConfigureByConvention();
            b.Property(x => x.NameAr).IsRequired().HasMaxLength(Holiday.MaxNameLength);
            b.Property(x => x.NameEn).IsRequired().HasMaxLength(Holiday.MaxNameLength);
            b.HasIndex(x => new { x.TenantId, x.Date }).IsUnique();
        });

        builder.Entity<Employee>(b =>
        {
            b.ToTable(Prefix + "Employees");
            b.ConfigureByConvention();
            b.Property(x => x.FullNameAr).IsRequired().HasMaxLength(Employee.MaxFullNameLength);
            b.Property(x => x.FullNameEn).HasMaxLength(Employee.MaxFullNameLength);
            b.Property(x => x.Email).HasMaxLength(Employee.MaxEmailLength);
            b.Property(x => x.PhoneNumber).HasMaxLength(Employee.MaxPhoneNumberLength);
            b.Property(x => x.Address).HasMaxLength(Employee.MaxAddressLength);
            b.Property(x => x.JobTitle).IsRequired().HasMaxLength(Employee.MaxJobTitleLength);
            b.Property(x => x.EmployeeNumber).HasMaxLength(Employee.MaxEmployeeNumberLength);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.Gender).HasConversion<string>().HasMaxLength(16);
            b.HasIndex(x => x.UserId).IsUnique().HasFilter("\"UserId\" IS NOT NULL");
            b.HasIndex(x => x.DepartmentId);
            b.HasIndex(x => x.ManagerEmployeeId);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.FullNameAr);
            b.HasIndex(x => x.FullNameEn);
            b.HasIndex(x => new { x.TenantId, x.EmployeeNumber }).IsUnique().HasFilter("\"EmployeeNumber\" IS NOT NULL");
        });

        builder.Entity<EmployeeManagerLink>(b =>
        {
            b.ToTable(Prefix + "EmployeeManagerLinks");
            b.ConfigureByConvention();
            b.Property(x => x.Reason).HasMaxLength(EmployeeManagerLink.MaxReasonLength);
            b.HasIndex(x => new { x.EmployeeId, x.ToDate });
        });

        builder.Entity<EmployeeShiftAssignment>(b =>
        {
            b.ToTable(Prefix + "EmployeeShiftAssignments");
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.EmployeeId, x.EndDate });
            b.HasIndex(x => new { x.WorkShiftId, x.EndDate });
        });

        builder.Entity<AdministrativeTaskLogEntry>(b =>
        {
            b.ToTable(Prefix + "AdministrativeTaskLog");
            b.ConfigureByConvention();
            b.Property(x => x.TaskType).HasConversion<string>().HasMaxLength(32);
            b.HasIndex(x => new { x.EmployeeId, x.OccurredAt });
        });

        builder.Entity<EmployeeDocument>(b =>
        {
            b.ToTable(Prefix + "EmployeeDocuments");
            b.ConfigureByConvention();
            b.Property(x => x.Title).IsRequired().HasMaxLength(EmployeeDocument.MaxTitleLength);
            b.Property(x => x.FileName).IsRequired().HasMaxLength(EmployeeDocument.MaxFileNameLength);
            b.Property(x => x.ContentType).IsRequired().HasMaxLength(EmployeeDocument.MaxContentTypeLength);
            b.Property(x => x.BlobName).IsRequired().HasMaxLength(EmployeeDocument.MaxBlobNameLength);
            b.Property(x => x.Type).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.Sensitivity).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.ScanStatus).HasConversion<string>().HasMaxLength(16);
            b.HasIndex(x => new { x.EmployeeId, x.Type });
        });

        builder.Entity<EmployeeDocumentAccessLog>(b =>
        {
            b.ToTable(Prefix + "EmployeeDocumentAccessLogs");
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.DocumentId, x.AccessedAt });
        });

        builder.Entity<FileDownloadLink>(b =>
        {
            b.ToTable(Prefix + "FileDownloadLinks");
            b.ConfigureByConvention();
            b.Property(x => x.TokenHash).IsRequired().HasMaxLength(FileDownloadLink.TokenHashLength);
            b.Property(x => x.BlobName).IsRequired().HasMaxLength(FileDownloadLink.MaxBlobNameLength);
            b.Property(x => x.FileName).IsRequired().HasMaxLength(FileDownloadLink.MaxFileNameLength);
            b.Property(x => x.ContentType).IsRequired().HasMaxLength(FileDownloadLink.MaxContentTypeLength);
            b.HasIndex(x => x.TokenHash).IsUnique();
            b.HasIndex(x => x.ResourceId);
            b.HasIndex(x => x.ExpiresAt);
        });

        builder.Entity<Delegation>(b =>
        {
            b.ToTable(Prefix + "Delegations");
            b.ConfigureByConvention();
            b.Property(x => x.Reason).IsRequired().HasMaxLength(Delegation.MaxReasonLength);
            b.Property(x => x.Scope).HasConversion<string>().HasMaxLength(32);
            b.HasIndex(x => new { x.FromUserId, x.EndsAt });
            b.HasIndex(x => new { x.ToUserId, x.EndsAt });
        });

        builder.Entity<AttendanceDevice>(b =>
        {
            b.ToTable(Prefix + "AttendanceDevices");
            b.ConfigureByConvention();
            b.Property(x => x.SerialNumber).IsRequired().HasMaxLength(AttendanceDevice.MaxSerialNumberLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(AttendanceDevice.MaxNameLength);
            b.Property(x => x.KeyHash).IsRequired().HasMaxLength(AttendanceDevice.KeyHashLength);
            b.HasIndex(x => new { x.TenantId, x.SerialNumber }).IsUnique();
            b.HasIndex(x => x.KeyHash).IsUnique();
            b.HasIndex(x => x.IsActive);
        });

        builder.Entity<AttendanceDeviceActivity>(b =>
        {
            b.ToTable(Prefix + "AttendanceDeviceActivity");
            b.ConfigureByConvention();
            b.Property(x => x.Id).ValueGeneratedNever();
        });

        builder.Entity<DeviceUserMap>(b =>
        {
            b.ToTable(Prefix + "DeviceUserMaps");
            b.ConfigureByConvention();
            b.Property(x => x.DeviceUserId).IsRequired().HasMaxLength(DeviceUserMap.MaxDeviceUserIdLength);
            b.HasIndex(x => new { x.DeviceId, x.DeviceUserId }).IsUnique();
            b.HasIndex(x => new { x.DeviceId, x.EmployeeId }).IsUnique();
            b.HasIndex(x => x.EmployeeId);
        });

        builder.Entity<AttendancePunch>(b =>
        {
            b.ToTable(Prefix + "AttendancePunches");
            b.ConfigureByConvention();
            b.Property(x => x.DeviceUserId).IsRequired().HasMaxLength(DeviceUserMap.MaxDeviceUserIdLength);
            b.Property(x => x.Source).HasConversion<string>().HasMaxLength(16);
            b.HasIndex(x => new { x.DeviceId, x.DeviceUserId, x.PunchTime }).IsUnique();
            b.HasIndex(x => new { x.EmployeeId, x.PunchTime });
            b.HasIndex(x => x.ImportId);
        });

        builder.Entity<AttendanceDay>(b =>
        {
            b.ToTable(Prefix + "AttendanceDays");
            b.ConfigureByConvention();
            b.Ignore(x => x.CheckIn);
            b.Ignore(x => x.CheckOut);
            b.Ignore(x => x.IsCorrected);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.LeaveDays).HasPrecision(9, 4);
            b.HasIndex(x => new { x.EmployeeId, x.Date }).IsUnique();
            b.HasIndex(x => new { x.Date, x.Status });
        });

        builder.Entity<AttendanceImport>(b =>
        {
            b.ToTable(Prefix + "AttendanceImports");
            b.ConfigureByConvention();
            b.Property(x => x.FileName).IsRequired().HasMaxLength(AttendanceImport.MaxFileNameLength);
            b.Property(x => x.BlobName).IsRequired().HasMaxLength(AttendanceImport.MaxBlobNameLength);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
            b.Property(x => x.ErrorsJson).IsRequired().HasColumnType("jsonb");
            b.HasIndex(x => new { x.Status, x.CreationTime });
        });

        builder.Entity<RequestType>(b =>
        {
            b.ToTable(Prefix + "RequestTypes");
            b.ConfigureByConvention();
            b.Property(x => x.Code).IsRequired().HasMaxLength(RequestType.MaxCodeLength);
            b.Property(x => x.NameAr).IsRequired().HasMaxLength(RequestType.MaxNameLength);
            b.Property(x => x.NameEn).IsRequired().HasMaxLength(RequestType.MaxNameLength);
            b.Property(x => x.Destination).HasConversion<string>().HasMaxLength(16);
            b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            b.HasIndex(x => x.IsActive);
        });

        builder.Entity<Request>(b =>
        {
            b.ToTable(Prefix + "Requests");
            b.ConfigureByConvention();
            b.Property(x => x.Destination).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
            b.Property(x => x.Description).HasMaxLength(Request.MaxDescriptionLength);
            b.Property(x => x.DetailsSummaryAr).IsRequired().HasMaxLength(Request.MaxSummaryLength);
            b.Property(x => x.DetailsSummaryEn).IsRequired().HasMaxLength(Request.MaxSummaryLength);
            b.Property(x => x.RejectionReason).HasMaxLength(Request.MaxNoteLength);
            b.Property(x => x.CurrentApproverRole).HasMaxLength(32);
            b.Property(x => x.Quantity).HasPrecision(18, 4);
            b.Ignore(x => x.IsPending);
            b.Ignore(x => x.CurrentStep);
            b.HasMany(x => x.Steps).WithOne().HasForeignKey(x => x.RequestId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Timeline).WithOne().HasForeignKey(x => x.RequestId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<RequestType>().WithMany().HasForeignKey(x => x.RequestTypeId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Employee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.EmployeeId, x.Status });
            b.HasIndex(x => new { x.OwnerUserId, x.CreationTime });
            b.HasIndex(x => new { x.Status, x.CreationTime });
            b.HasIndex(x => new { x.CurrentApproverUserId, x.CurrentApproverRole });
            b.HasIndex(x => x.RequestTypeId);
        });

        builder.Entity<RequestStep>(b =>
        {
            b.ToTable(Prefix + "RequestSteps");
            b.ConfigureByConvention();
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.Stage).HasConversion<string>().HasMaxLength(24);
            b.Property(x => x.ApproverRole).IsRequired().HasMaxLength(32);
            b.Property(x => x.Decision).HasConversion<string>().HasMaxLength(24);
            b.Property(x => x.Note).HasMaxLength(Request.MaxNoteLength);
            b.HasIndex(x => new { x.RequestId, x.Order }).IsUnique();
        });

        builder.Entity<RequestTimelineEntry>(b =>
        {
            b.ToTable(Prefix + "RequestTimelineEntries");
            b.ConfigureByConvention();
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.Action).IsRequired().HasMaxLength(RequestTimelineEntry.MaxActionLength);
            b.Property(x => x.Note).HasMaxLength(Request.MaxNoteLength);
            b.HasIndex(x => new { x.RequestId, x.OccurredAt });
        });

        ConfigureLeaves(builder);
        ConfigureMemos(builder);
    }

    private static void ConfigureMemos(ModelBuilder builder)
    {
        builder.Entity<HR_Mangment2.Memos.Memo>(b =>
        {
            b.ToTable(Prefix + "Memos");
            b.ConfigureByConvention();
            b.Property(x => x.Kind).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
            b.Property(x => x.WriterRow).HasConversion<string>().HasMaxLength(24);
            b.Property(x => x.Content).IsRequired().HasMaxLength(HR_Mangment2.Memos.Memo.MaxContentLength);
            b.Property(x => x.CurrentHandlerRole).HasMaxLength(32);
            b.Property(x => x.CloseReason).HasMaxLength(HR_Mangment2.Memos.Memo.MaxNoteLength);
            b.Ignore(x => x.IsFinal);
            b.Ignore(x => x.IsAtLastStage);
            b.Ignore(x => x.CanForwardToHr);
            b.HasMany(x => x.Handlers).WithOne().HasForeignKey(x => x.MemoId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Timeline).WithOne().HasForeignKey(x => x.MemoId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Employee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.WriterUserId, x.CreationTime });
            b.HasIndex(x => new { x.EmployeeId, x.Status });
            b.HasIndex(x => new { x.CurrentHandlerUserId, x.CurrentHandlerRole });
        });

        builder.Entity<HR_Mangment2.Memos.MemoHandler>(b =>
        {
            b.ToTable(Prefix + "MemoHandlers");
            b.ConfigureByConvention();
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.HandlerRole).IsRequired().HasMaxLength(32);
            b.HasIndex(x => x.MemoId);
            b.HasIndex(x => new { x.HandlerUserId, x.HandlerRole });
        });

        builder.Entity<HR_Mangment2.Memos.MemoTimelineEntry>(b =>
        {
            b.ToTable(Prefix + "MemoTimelineEntries");
            b.ConfigureByConvention();
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.Action).IsRequired().HasMaxLength(HR_Mangment2.Memos.MemoTimelineEntry.MaxActionLength);
            b.Property(x => x.Note).HasMaxLength(HR_Mangment2.Memos.Memo.MaxNoteLength);
            b.HasIndex(x => new { x.MemoId, x.OccurredAt });
        });

        builder.Entity<HR_Mangment2.Memos.Incident>(b =>
        {
            b.ToTable(Prefix + "Incidents");
            b.ConfigureByConvention();
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
            b.Property(x => x.Description).IsRequired().HasMaxLength(HR_Mangment2.Memos.Incident.MaxDescriptionLength);
            b.Ignore(x => x.CurrentHandlerRole);
            b.HasMany(x => x.Timeline).WithOne().HasForeignKey(x => x.IncidentId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Employee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.SubmitterUserId, x.CreationTime });
            b.HasIndex(x => new { x.EmployeeId, x.Status });
            b.HasIndex(x => new { x.Status, x.CreationTime });
            b.HasIndex(x => x.CurrentManagerUserId);
        });

        builder.Entity<HR_Mangment2.Memos.IncidentTimelineEntry>(b =>
        {
            b.ToTable(Prefix + "IncidentTimelineEntries");
            b.ConfigureByConvention();
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.Action).IsRequired().HasMaxLength(HR_Mangment2.Memos.IncidentTimelineEntry.MaxActionLength);
            b.Property(x => x.Note).HasMaxLength(HR_Mangment2.Memos.Incident.MaxNoteLength);
            b.HasIndex(x => new { x.IncidentId, x.OccurredAt });
        });
    }

    private static void ConfigureLeaves(ModelBuilder builder)
    {
        builder.Entity<OtherLeaveType>(b =>
        {
            b.ToTable(Prefix + "OtherLeaveTypes");
            b.ConfigureByConvention();
            b.Property(x => x.NameAr).IsRequired().HasMaxLength(OtherLeaveType.MaxNameLength);
            b.Property(x => x.NameEn).IsRequired().HasMaxLength(OtherLeaveType.MaxNameLength);
            b.Property(x => x.MaxDays).HasPrecision(9, 4);
            b.Property(x => x.EligibleCategory).HasConversion<string>().HasMaxLength(16);
            b.HasIndex(x => x.IsActive);
        });

        builder.Entity<EmployeeLeaveAccount>(b =>
        {
            b.ToTable(Prefix + "EmployeeLeaveAccounts");
            b.ConfigureByConvention();
            b.Property(x => x.NextYearEntitlementDays).HasPrecision(9, 4);
            b.HasOne<Employee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.EmployeeId).IsUnique();
        });

        builder.Entity<EmployeeLeaveYear>(b =>
        {
            b.ToTable(Prefix + "EmployeeLeaveYears");
            b.ConfigureByConvention();
            b.Property(x => x.EntitlementDays).HasPrecision(9, 4);
            b.HasOne<Employee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.EmployeeId, x.YearStart }).IsUnique();
            b.HasIndex(x => new { x.IsGranted, x.IsClosed, x.YearEnd });
        });

        builder.Entity<LeaveLedgerEntry>(b =>
        {
            b.ToTable(Prefix + "LeaveLedgerEntries");
            b.ConfigureByConvention();
            b.Property(x => x.Type).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.Bucket).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.Days).HasPrecision(9, 4);
            b.Property(x => x.Hours).HasPrecision(9, 4);
            b.Property(x => x.HourlyMonth).HasMaxLength(LeaveLedgerEntry.MonthLength);
            b.Property(x => x.Reason).HasMaxLength(LeaveLedgerEntry.MaxReasonLength);
            b.HasOne<EmployeeLeaveYear>().WithMany().HasForeignKey(x => x.LeaveYearId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.EmployeeId, x.CreationTime });
            b.HasIndex(x => new { x.EmployeeId, x.Bucket, x.HourlyMonth });
            b.HasIndex(x => x.LeaveYearId);
            b.HasIndex(x => x.RequestId);
        });

        builder.Entity<LeaveRequest>(b =>
        {
            b.ToTable(Prefix + "LeaveRequests");
            b.ConfigureByConvention();
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.Kind).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.State).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.BalanceEffect).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.Notes).HasMaxLength(LeaveRequest.MaxNotesLength);
            b.Property(x => x.WorkingDays).HasPrecision(9, 4);
            b.Property(x => x.RequestedHours).HasPrecision(9, 4);
            b.Property(x => x.HourlyQuotaHoursUsed).HasPrecision(9, 4);
            b.Property(x => x.HourlyExcessHours).HasPrecision(9, 4);
            b.Property(x => x.AnnualDays).HasPrecision(9, 4);
            b.Property(x => x.FromCarriedDays).HasPrecision(9, 4);
            b.Property(x => x.FromEntitlementDays).HasPrecision(9, 4);
            b.HasOne<Request>().WithOne().HasForeignKey<LeaveRequest>(x => x.Id).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<OtherLeaveType>().WithMany().HasForeignKey(x => x.OtherLeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Segments).WithOne().HasForeignKey(x => x.LeaveRequestId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.EmployeeId, x.State, x.RangeStart, x.RangeEnd });
        });

        builder.Entity<LeaveRequestSegment>(b =>
        {
            b.ToTable(Prefix + "LeaveRequestSegments");
            b.ConfigureByConvention();
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.WorkingDays).HasPrecision(9, 4);
            b.Property(x => x.FromCarriedDays).HasPrecision(9, 4);
            b.Property(x => x.FromEntitlementDays).HasPrecision(9, 4);
            b.HasIndex(x => new { x.LeaveRequestId, x.YearStart });
        });

        builder.Entity<LeaveAttachment>(b =>
        {
            b.ToTable(Prefix + "LeaveAttachments");
            b.ConfigureByConvention();
            b.Property(x => x.FileName).IsRequired().HasMaxLength(LeaveAttachment.MaxFileNameLength);
            b.Property(x => x.ContentType).IsRequired().HasMaxLength(LeaveAttachment.MaxContentTypeLength);
            b.Property(x => x.BlobName).IsRequired().HasMaxLength(LeaveAttachment.MaxBlobNameLength);
            b.HasIndex(x => x.LeaveRequestId);
            b.HasIndex(x => new { x.LeaveRequestId, x.CreationTime });
        });

        builder.Entity<AttendanceCorrection>(b =>
        {
            b.ToTable(Prefix + "AttendanceCorrections");
            b.ConfigureByConvention();
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.PunchType).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.Reason).IsRequired().HasMaxLength(AttendanceCorrection.MaxReasonLength);
            b.Property(x => x.SettlementPeriod).HasMaxLength(AttendanceCorrection.SettlementPeriodLength);
            b.HasOne<Request>().WithOne().HasForeignKey<AttendanceCorrection>(x => x.Id).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.EmployeeId, x.Date, x.PunchType, x.IsPending });
        });
    }
}
