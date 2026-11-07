using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_Mangment2.Migrations
{
    /// <inheritdoc />
    public partial class Hr_EmployeesDocumentsDelegations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppAdministrativeTaskLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TaskType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAdministrativeTaskLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppDelegations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    FromUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Scope = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartsAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDelegations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppEmployeeDocumentAccessLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEmployeeDocumentAccessLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppEmployeeDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Sensitivity = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ScanStatus = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    BlobName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEmployeeDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppEmployeeManagerLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManagerEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ToDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEmployeeManagerLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppEmployees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    FullNameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FullNameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobTitle = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    HireDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ManagerEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Gender = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    EmployeeNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEmployees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppEmployeeShiftAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkShiftId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEmployeeShiftAssignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppFileDownloadLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlobName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppFileDownloadLinks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAdministrativeTaskLog_EmployeeId_OccurredAt",
                table: "AppAdministrativeTaskLog",
                columns: new[] { "EmployeeId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppDelegations_FromUserId_EndsAt",
                table: "AppDelegations",
                columns: new[] { "FromUserId", "EndsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppDelegations_ToUserId_EndsAt",
                table: "AppDelegations",
                columns: new[] { "ToUserId", "EndsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployeeDocumentAccessLogs_DocumentId_AccessedAt",
                table: "AppEmployeeDocumentAccessLogs",
                columns: new[] { "DocumentId", "AccessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployeeDocuments_EmployeeId_Type",
                table: "AppEmployeeDocuments",
                columns: new[] { "EmployeeId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployeeManagerLinks_EmployeeId_ToDate",
                table: "AppEmployeeManagerLinks",
                columns: new[] { "EmployeeId", "ToDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployees_DepartmentId",
                table: "AppEmployees",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployees_EmployeeNumber",
                table: "AppEmployees",
                column: "EmployeeNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployees_FullNameAr",
                table: "AppEmployees",
                column: "FullNameAr");

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployees_FullNameEn",
                table: "AppEmployees",
                column: "FullNameEn");

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployees_ManagerEmployeeId",
                table: "AppEmployees",
                column: "ManagerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployees_Status",
                table: "AppEmployees",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployees_UserId",
                table: "AppEmployees",
                column: "UserId",
                unique: true,
                filter: "\"UserId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployeeShiftAssignments_EmployeeId_EndDate",
                table: "AppEmployeeShiftAssignments",
                columns: new[] { "EmployeeId", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployeeShiftAssignments_WorkShiftId_EndDate",
                table: "AppEmployeeShiftAssignments",
                columns: new[] { "WorkShiftId", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppFileDownloadLinks_ExpiresAt",
                table: "AppFileDownloadLinks",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_AppFileDownloadLinks_ResourceId",
                table: "AppFileDownloadLinks",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_AppFileDownloadLinks_TokenHash",
                table: "AppFileDownloadLinks",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAdministrativeTaskLog");

            migrationBuilder.DropTable(
                name: "AppDelegations");

            migrationBuilder.DropTable(
                name: "AppEmployeeDocumentAccessLogs");

            migrationBuilder.DropTable(
                name: "AppEmployeeDocuments");

            migrationBuilder.DropTable(
                name: "AppEmployeeManagerLinks");

            migrationBuilder.DropTable(
                name: "AppEmployees");

            migrationBuilder.DropTable(
                name: "AppEmployeeShiftAssignments");

            migrationBuilder.DropTable(
                name: "AppFileDownloadLinks");
        }
    }
}
