using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_Mangment2.Migrations
{
    /// <inheritdoc />
    public partial class Hr_Complaints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "complaints");

            migrationBuilder.CreateTable(
                name: "ComplaintAccessLogs",
                schema: "complaints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ComplaintId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintAccessLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintHandlerAssignments",
                schema: "complaints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    From = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    To = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PermissionsReleased = table.Column<bool>(type: "boolean", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintHandlerAssignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Complaints",
                schema: "complaints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ComplainantEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComplainantUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetKind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    RespondentEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    RespondentUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RespondentCategory = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    MinimumLevel = table.Column<int>(type: "integer", nullable: false),
                    LevelRaisedBySystem = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DepartmentManagerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    WrappedKey = table.Column<byte[]>(type: "bytea", nullable: false),
                    EncryptedSubject = table.Column<byte[]>(type: "bytea", nullable: false),
                    EncryptedContent = table.Column<byte[]>(type: "bytea", nullable: false),
                    EncryptedClosureDecision = table.Column<byte[]>(type: "bytea", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastOverdueReminderAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Complaints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintActions",
                schema: "complaints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ComplaintId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncryptedNote = table.Column<byte[]>(type: "bytea", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintActions_Complaints_ComplaintId",
                        column: x => x.ComplaintId,
                        principalSchema: "complaints",
                        principalTable: "Complaints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintAttachments",
                schema: "complaints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ComplaintId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncryptedFileName = table.Column<byte[]>(type: "bytea", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    BlobName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintAttachments_Complaints_ComplaintId",
                        column: x => x.ComplaintId,
                        principalSchema: "complaints",
                        principalTable: "Complaints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintAccessLogs_ComplaintId_At",
                schema: "complaints",
                table: "ComplaintAccessLogs",
                columns: new[] { "ComplaintId", "At" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintActions_ComplaintId_CreationTime",
                schema: "complaints",
                table: "ComplaintActions",
                columns: new[] { "ComplaintId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintAttachments_ComplaintId",
                schema: "complaints",
                table: "ComplaintAttachments",
                column: "ComplaintId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintHandlerAssignments_Status_To",
                schema: "complaints",
                table: "ComplaintHandlerAssignments",
                columns: new[] { "Status", "To" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintHandlerAssignments_UserId_Status",
                schema: "complaints",
                table: "ComplaintHandlerAssignments",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_ComplainantEmployeeId",
                schema: "complaints",
                table: "Complaints",
                column: "ComplainantEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_ComplainantUserId_SubmittedAt",
                schema: "complaints",
                table: "Complaints",
                columns: new[] { "ComplainantUserId", "SubmittedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_DepartmentManagerUserId_Status",
                schema: "complaints",
                table: "Complaints",
                columns: new[] { "DepartmentManagerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_Level_Status_SubmittedAt",
                schema: "complaints",
                table: "Complaints",
                columns: new[] { "Level", "Status", "SubmittedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_Status_DueAt",
                schema: "complaints",
                table: "Complaints",
                columns: new[] { "Status", "DueAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_TenantId_ReferenceNumber",
                schema: "complaints",
                table: "Complaints",
                columns: new[] { "TenantId", "ReferenceNumber" },
                unique: true);

            migrationBuilder.Sql(RestrictedAccountSql);
        }

        /// <summary>
        /// The restricted account for the Complaints connection string. It cannot log in until operations give it a
        /// password, cannot delete anything, cannot change the access log, and sees only its own company's rows.
        /// </summary>
        private const string RestrictedAccountSql = """
            DO $$
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'hr_complaints_app') THEN
                    CREATE ROLE hr_complaints_app NOLOGIN;
                END IF;
            END
            $$;

            GRANT USAGE ON SCHEMA complaints TO hr_complaints_app;
            GRANT SELECT, INSERT, UPDATE ON complaints."Complaints", complaints."ComplaintHandlerAssignments" TO hr_complaints_app;
            GRANT SELECT, INSERT ON complaints."ComplaintActions", complaints."ComplaintAttachments", complaints."ComplaintAccessLogs" TO hr_complaints_app;

            ALTER TABLE complaints."Complaints" ENABLE ROW LEVEL SECURITY;
            ALTER TABLE complaints."ComplaintActions" ENABLE ROW LEVEL SECURITY;
            ALTER TABLE complaints."ComplaintAttachments" ENABLE ROW LEVEL SECURITY;
            ALTER TABLE complaints."ComplaintAccessLogs" ENABLE ROW LEVEL SECURITY;
            ALTER TABLE complaints."ComplaintHandlerAssignments" ENABLE ROW LEVEL SECURITY;

            CREATE POLICY tenant_isolation ON complaints."Complaints" TO hr_complaints_app
                USING ("TenantId"::text = current_setting('hr.tenant_id', true)) WITH CHECK ("TenantId"::text = current_setting('hr.tenant_id', true));
            CREATE POLICY tenant_isolation ON complaints."ComplaintActions" TO hr_complaints_app
                USING ("TenantId"::text = current_setting('hr.tenant_id', true)) WITH CHECK ("TenantId"::text = current_setting('hr.tenant_id', true));
            CREATE POLICY tenant_isolation ON complaints."ComplaintAttachments" TO hr_complaints_app
                USING ("TenantId"::text = current_setting('hr.tenant_id', true)) WITH CHECK ("TenantId"::text = current_setting('hr.tenant_id', true));
            CREATE POLICY tenant_isolation ON complaints."ComplaintAccessLogs" TO hr_complaints_app
                USING ("TenantId"::text = current_setting('hr.tenant_id', true)) WITH CHECK ("TenantId"::text = current_setting('hr.tenant_id', true));
            CREATE POLICY tenant_isolation ON complaints."ComplaintHandlerAssignments" TO hr_complaints_app
                USING ("TenantId"::text = current_setting('hr.tenant_id', true)) WITH CHECK ("TenantId"::text = current_setting('hr.tenant_id', true));
            """;

        private const string RevokeRestrictedAccountSql = """
            REVOKE ALL ON ALL TABLES IN SCHEMA complaints FROM hr_complaints_app;
            REVOKE USAGE ON SCHEMA complaints FROM hr_complaints_app;
            """;

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(RevokeRestrictedAccountSql);

            migrationBuilder.DropTable(
                name: "ComplaintAccessLogs",
                schema: "complaints");

            migrationBuilder.DropTable(
                name: "ComplaintActions",
                schema: "complaints");

            migrationBuilder.DropTable(
                name: "ComplaintAttachments",
                schema: "complaints");

            migrationBuilder.DropTable(
                name: "ComplaintHandlerAssignments",
                schema: "complaints");

            migrationBuilder.DropTable(
                name: "Complaints",
                schema: "complaints");
        }
    }
}
