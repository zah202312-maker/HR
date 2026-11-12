using HR_Mangment2.Complaints.EntityFrameworkCore;
using HR_Mangment2.Delegations;
using HR_Mangment2.Departments;
using HR_Mangment2.Employees;
using HR_Mangment2.Files;
using HR_Mangment2.Finance.EntityFrameworkCore;
using HR_Mangment2.Holidays;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Notifications;
using HR_Mangment2.Settings;
using HR_Mangment2.WorkShifts;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace HR_Mangment2.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class HR_Mangment2DbContext :
    AbpDbContext<HR_Mangment2DbContext>,
    ITenantManagementDbContext,
    IIdentityDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */


    #region Business entities

    public DbSet<Notification> Notifications { get; set; }
    public DbSet<DeviceToken> DeviceTokens { get; set; }
    public DbSet<SystemSettingSection> SystemSettingSections { get; set; }
    public DbSet<HR_Mangment2.Auditing.HrAuditEntry> HrAuditEntries { get; set; }
    public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<WorkShift> WorkShifts { get; set; }
    public DbSet<Holiday> Holidays { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeManagerLink> EmployeeManagerLinks { get; set; }
    public DbSet<EmployeeShiftAssignment> EmployeeShiftAssignments { get; set; }
    public DbSet<AdministrativeTaskLogEntry> AdministrativeTaskLogEntries { get; set; }
    public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }
    public DbSet<EmployeeDocumentAccessLog> EmployeeDocumentAccessLogs { get; set; }
    public DbSet<FileDownloadLink> FileDownloadLinks { get; set; }
    public DbSet<Delegation> Delegations { get; set; }
    public DbSet<HR_Mangment2.Attendance.AttendanceDevice> AttendanceDevices { get; set; }
    public DbSet<HR_Mangment2.Attendance.AttendanceDeviceActivity> AttendanceDeviceActivity { get; set; }
    public DbSet<HR_Mangment2.Attendance.DeviceUserMap> DeviceUserMaps { get; set; }
    public DbSet<HR_Mangment2.Attendance.AttendancePunch> AttendancePunches { get; set; }
    public DbSet<HR_Mangment2.Attendance.AttendanceDay> AttendanceDays { get; set; }
    public DbSet<HR_Mangment2.Attendance.AttendanceImport> AttendanceImports { get; set; }
    public DbSet<HR_Mangment2.Requests.RequestType> RequestTypes { get; set; }
    public DbSet<HR_Mangment2.Requests.Request> Requests { get; set; }
    public DbSet<HR_Mangment2.Requests.RequestStep> RequestSteps { get; set; }
    public DbSet<HR_Mangment2.Requests.RequestTimelineEntry> RequestTimelineEntries { get; set; }
    public DbSet<HR_Mangment2.Memos.Memo> Memos { get; set; }
    public DbSet<HR_Mangment2.Memos.MemoHandler> MemoHandlers { get; set; }
    public DbSet<HR_Mangment2.Memos.MemoTimelineEntry> MemoTimelineEntries { get; set; }
    public DbSet<HR_Mangment2.Memos.Incident> Incidents { get; set; }
    public DbSet<HR_Mangment2.Memos.IncidentTimelineEntry> IncidentTimelineEntries { get; set; }
    public DbSet<HR_Mangment2.Leaves.OtherLeaveType> OtherLeaveTypes { get; set; }
    public DbSet<HR_Mangment2.Leaves.EmployeeLeaveAccount> EmployeeLeaveAccounts { get; set; }
    public DbSet<HR_Mangment2.Leaves.EmployeeLeaveYear> EmployeeLeaveYears { get; set; }
    public DbSet<HR_Mangment2.Leaves.LeaveLedgerEntry> LeaveLedgerEntries { get; set; }
    public DbSet<HR_Mangment2.Leaves.LeaveRequest> LeaveRequests { get; set; }
    public DbSet<HR_Mangment2.Leaves.LeaveRequestSegment> LeaveRequestSegments { get; set; }
    public DbSet<HR_Mangment2.Leaves.LeaveAttachment> LeaveAttachments { get; set; }
    public DbSet<HR_Mangment2.Attendance.AttendanceCorrection> AttendanceCorrections { get; set; }
    public DbSet<HR_Mangment2.Store.Item> Items { get; set; }
    public DbSet<HR_Mangment2.Store.StockMovement> StockMovements { get; set; }
    public DbSet<HR_Mangment2.Store.StockIssue> StockIssues { get; set; }
    public DbSet<HR_Mangment2.Store.Custody> Custodies { get; set; }
    public DbSet<HR_Mangment2.Store.CustodyCount> CustodyCounts { get; set; }
    public DbSet<HR_Mangment2.Store.CustodyCountLine> CustodyCountLines { get; set; }
    public DbSet<HR_Mangment2.Store.ItemCategory> ItemCategories { get; set; }
    public DbSet<HR_Mangment2.Store.ItemUnit> ItemUnits { get; set; }
    public DbSet<HR_Mangment2.Store.StoreTeam> StoreTeams { get; set; }
    public DbSet<HR_Mangment2.Store.StorePurchase> StorePurchases { get; set; }
    public DbSet<HR_Mangment2.Store.StorePurchaseLine> StorePurchaseLines { get; set; }
    public DbSet<HR_Mangment2.Store.StoreIssueVoucher> StoreIssueVouchers { get; set; }
    public DbSet<HR_Mangment2.Store.StoreIssueVoucherLine> StoreIssueVoucherLines { get; set; }
    public DbSet<HR_Mangment2.Store.StoreDocumentAttachment> StoreDocumentAttachments { get; set; }
    public DbSet<HR_Mangment2.Evaluations.EvaluationCriterion> EvaluationCriteria { get; set; }
    public DbSet<HR_Mangment2.Evaluations.EvaluationGrade> EvaluationGrades { get; set; }
    public DbSet<HR_Mangment2.Evaluations.EvaluationCycle> EvaluationCycles { get; set; }
    public DbSet<HR_Mangment2.Evaluations.EvaluationCycleCriterion> EvaluationCycleCriteria { get; set; }
    public DbSet<HR_Mangment2.Evaluations.EvaluationCycleGrade> EvaluationCycleGrades { get; set; }
    public DbSet<HR_Mangment2.Evaluations.Evaluation> Evaluations { get; set; }
    public DbSet<HR_Mangment2.Evaluations.EvaluationScore> EvaluationScores { get; set; }
    public DbSet<HR_Mangment2.Evaluations.EvaluationResponse> EvaluationResponses { get; set; }
    public DbSet<HR_Mangment2.Evaluations.EvaluationInternalNote> EvaluationInternalNotes { get; set; }
    public DbSet<HR_Mangment2.Evaluations.EvaluationInternalNoteDraft> EvaluationInternalNoteDrafts { get; set; }
    public DbSet<HR_Mangment2.Evaluations.DepartmentReport> DepartmentReports { get; set; }
    public DbSet<EmployeeHireDateCorrection> EmployeeHireDateCorrections { get; set; }
    public DbSet<HR_Mangment2.EmploymentContracts.EmploymentContract> EmploymentContracts { get; set; }
    public DbSet<HR_Mangment2.Warnings.Warning> Warnings { get; set; }
    public DbSet<HR_Mangment2.Warnings.WarningAttachment> WarningAttachments { get; set; }
    public DbSet<HR_Mangment2.Appreciation.AppreciationLetter> AppreciationLetters { get; set; }
    public DbSet<HR_Mangment2.Evaluations.Circular> Circulars { get; set; }
    public DbSet<HR_Mangment2.Evaluations.CircularRecipient> CircularRecipients { get; set; }
    public DbSet<HR_Mangment2.ReviewReports.ReviewReport> ReviewReports { get; set; }
    public DbSet<HR_Mangment2.ReviewReports.ReviewReportTeam> ReviewReportTeams { get; set; }
    public DbSet<HR_Mangment2.ReviewReports.ReviewRoom> ReviewRooms { get; set; }
    public DbSet<HR_Mangment2.EmploymentContracts.ContractAttachment> ContractAttachments { get; set; }
    public DbSet<HR_Mangment2.HrEvaluations.HrEvaluationCriterion> HrEvaluationCriteria { get; set; }
    public DbSet<HR_Mangment2.HrEvaluations.HrEvaluation> HrEvaluations { get; set; }
    public DbSet<HR_Mangment2.HrEvaluations.HrEvaluationScore> HrEvaluationScores { get; set; }
    public DbSet<HR_Mangment2.HrEvaluations.HrEvaluationReport> HrEvaluationReports { get; set; }
    public DbSet<HR_Mangment2.HrEvaluations.HrEvaluationAlert> HrEvaluationAlerts { get; set; }
    public DbSet<HR_Mangment2.Identity.ProfilePhoto> ProfilePhotos { get; set; }
    public DbSet<HR_Mangment2.ReviewerEvaluations.ReviewerEvaluationCriterion> ReviewerEvaluationCriteria { get; set; }
    public DbSet<HR_Mangment2.ReviewerEvaluations.ReviewerEvaluation> ReviewerEvaluations { get; set; }
    public DbSet<HR_Mangment2.ReviewerEvaluations.ReviewerEvaluationItem> ReviewerEvaluationItems { get; set; }
    public DbSet<HR_Mangment2.ReviewerEvaluations.ReviewerEvaluationReport> ReviewerEvaluationReports { get; set; }
    public DbSet<HR_Mangment2.Notices.Notice> Notices { get; set; }
    public DbSet<HR_Mangment2.Notices.NoticeRecipient> NoticeRecipients { get; set; }

    #endregion

    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext and ISaasDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext and ISaasDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public HR_Mangment2DbContext(DbContextOptions<HR_Mangment2DbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();
        builder.ConfigureBlobStoring();

        // Module tables keep their own schemas, and the migrations stay here in one history.
        builder.ConfigureHr();

        builder.ConfigureComplaints();
        builder.ConfigureFinance();

        /* Configure your own tables/entities inside here */

        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(HR_Mangment2Consts.DbTablePrefix + "YourEntities", HR_Mangment2Consts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});
    }
}
