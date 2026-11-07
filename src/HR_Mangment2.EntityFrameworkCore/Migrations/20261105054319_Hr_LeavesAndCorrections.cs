using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_Mangment2.Migrations
{
    /// <inheritdoc />
    public partial class Hr_LeavesAndCorrections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppAttendanceCorrections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    PunchType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CorrectTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsPending = table.Column<bool>(type: "boolean", nullable: false),
                    OriginalValue = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReplacedValue = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AppliedValue = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AffectsClosedMonth = table.Column<bool>(type: "boolean", nullable: true),
                    SettlementPeriod = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAttendanceCorrections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAttendanceCorrections_AppRequests_Id",
                        column: x => x.Id,
                        principalTable: "AppRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppEmployeeLeaveAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    NextYearEntitlementDays = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEmployeeLeaveAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppEmployeeLeaveAccounts_AppEmployees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "AppEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppEmployeeLeaveYears",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    YearStart = table.Column<DateOnly>(type: "date", nullable: false),
                    YearEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    EntitlementDays = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    IsGranted = table.Column<bool>(type: "boolean", nullable: false),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEmployeeLeaveYears", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppEmployeeLeaveYears_AppEmployees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "AppEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppLeaveAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    BlobName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLeaveAttachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppOtherLeaveTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    NameAr = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    MaxDays = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    RequiresAttachment = table.Column<bool>(type: "boolean", nullable: false),
                    EligibleCategory = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    DeductsFromAnnualBalance = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppOtherLeaveTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppLeaveLedgerEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveYearId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Bucket = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Days = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    Hours = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    HourlyMonth = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLeaveLedgerEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppLeaveLedgerEntries_AppEmployeeLeaveYears_LeaveYearId",
                        column: x => x.LeaveYearId,
                        principalTable: "AppEmployeeLeaveYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppLeaveRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    FromTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    ToTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RangeStart = table.Column<DateOnly>(type: "date", nullable: false),
                    RangeEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    OtherLeaveTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    State = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    BalanceEffect = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    WorkingDays = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    RequestedHours = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    HourlyQuotaHoursUsed = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    HourlyExcessHours = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    AnnualDays = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    FromCarriedDays = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    FromEntitlementDays = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLeaveRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppLeaveRequests_AppOtherLeaveTypes_OtherLeaveTypeId",
                        column: x => x.OtherLeaveTypeId,
                        principalTable: "AppOtherLeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppLeaveRequests_AppRequests_Id",
                        column: x => x.Id,
                        principalTable: "AppRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppLeaveRequestSegments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaveRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    YearStart = table.Column<DateOnly>(type: "date", nullable: false),
                    YearEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    WorkingDays = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    FromCarriedDays = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    FromEntitlementDays = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLeaveRequestSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppLeaveRequestSegments_AppLeaveRequests_LeaveRequestId",
                        column: x => x.LeaveRequestId,
                        principalTable: "AppLeaveRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAttendanceCorrections_EmployeeId_Date_PunchType_IsPending",
                table: "AppAttendanceCorrections",
                columns: new[] { "EmployeeId", "Date", "PunchType", "IsPending" });

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployeeLeaveAccounts_EmployeeId",
                table: "AppEmployeeLeaveAccounts",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployeeLeaveYears_EmployeeId_YearStart",
                table: "AppEmployeeLeaveYears",
                columns: new[] { "EmployeeId", "YearStart" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployeeLeaveYears_IsGranted_IsClosed_YearEnd",
                table: "AppEmployeeLeaveYears",
                columns: new[] { "IsGranted", "IsClosed", "YearEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_AppLeaveAttachments_LeaveRequestId",
                table: "AppLeaveAttachments",
                column: "LeaveRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_AppLeaveAttachments_LeaveRequestId_CreationTime",
                table: "AppLeaveAttachments",
                columns: new[] { "LeaveRequestId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppLeaveLedgerEntries_EmployeeId_Bucket_HourlyMonth",
                table: "AppLeaveLedgerEntries",
                columns: new[] { "EmployeeId", "Bucket", "HourlyMonth" });

            migrationBuilder.CreateIndex(
                name: "IX_AppLeaveLedgerEntries_EmployeeId_CreationTime",
                table: "AppLeaveLedgerEntries",
                columns: new[] { "EmployeeId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppLeaveLedgerEntries_LeaveYearId",
                table: "AppLeaveLedgerEntries",
                column: "LeaveYearId");

            migrationBuilder.CreateIndex(
                name: "IX_AppLeaveLedgerEntries_RequestId",
                table: "AppLeaveLedgerEntries",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_AppLeaveRequests_EmployeeId_State_RangeStart_RangeEnd",
                table: "AppLeaveRequests",
                columns: new[] { "EmployeeId", "State", "RangeStart", "RangeEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_AppLeaveRequests_OtherLeaveTypeId",
                table: "AppLeaveRequests",
                column: "OtherLeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppLeaveRequestSegments_LeaveRequestId_YearStart",
                table: "AppLeaveRequestSegments",
                columns: new[] { "LeaveRequestId", "YearStart" });

            migrationBuilder.CreateIndex(
                name: "IX_AppOtherLeaveTypes_IsActive",
                table: "AppOtherLeaveTypes",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAttendanceCorrections");

            migrationBuilder.DropTable(
                name: "AppEmployeeLeaveAccounts");

            migrationBuilder.DropTable(
                name: "AppLeaveAttachments");

            migrationBuilder.DropTable(
                name: "AppLeaveLedgerEntries");

            migrationBuilder.DropTable(
                name: "AppLeaveRequestSegments");

            migrationBuilder.DropTable(
                name: "AppEmployeeLeaveYears");

            migrationBuilder.DropTable(
                name: "AppLeaveRequests");

            migrationBuilder.DropTable(
                name: "AppOtherLeaveTypes");
        }
    }
}
