using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_Mangment2.Migrations
{
    /// <inheritdoc />
    public partial class Hr_EvaluationsReportsCirculars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppCirculars",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Scope = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    SenderUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCirculars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppEvaluationCriteria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AppEvaluationCriteria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppEvaluationCycles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    OpensAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataFreezeAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EscalatesAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FrozenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastReminderAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EscalatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEvaluationCycles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppEvaluationGrades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AppEvaluationGrades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppCircularRecipients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CircularId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCircularRecipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCircularRecipients_AppCirculars_CircularId",
                        column: x => x.CircularId,
                        principalTable: "AppCirculars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppCircularRecipients_AppEmployees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "AppEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppDepartmentReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CycleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManagerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    SnapshotJson = table.Column<string>(type: "text", nullable: false),
                    EvaluationPinJson = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDepartmentReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppDepartmentReports_AppEvaluationCycles_CycleId",
                        column: x => x.CycleId,
                        principalTable: "AppEvaluationCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppEvaluationCycleCriteria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CycleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriterionId = table.Column<Guid>(type: "uuid", nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEvaluationCycleCriteria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppEvaluationCycleCriteria_AppEvaluationCycles_CycleId",
                        column: x => x.CycleId,
                        principalTable: "AppEvaluationCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppEvaluationCycleGrades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CycleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEvaluationCycleGrades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppEvaluationCycleGrades_AppEvaluationCycles_CycleId",
                        column: x => x.CycleId,
                        principalTable: "AppEvaluationCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppEvaluations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CycleId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    EmployeeNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ObjectionStatus = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    AttendanceFrozenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScheduledWorkingDays = table.Column<int>(type: "integer", nullable: false),
                    PresentDays = table.Column<int>(type: "integer", nullable: false),
                    AbsentDays = table.Column<int>(type: "integer", nullable: false),
                    LateCount = table.Column<int>(type: "integer", nullable: false),
                    LateMinutes = table.Column<int>(type: "integer", nullable: false),
                    LeaveDays = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEvaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppEvaluations_AppEmployees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "AppEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppEvaluations_AppEvaluationCycles_CycleId",
                        column: x => x.CycleId,
                        principalTable: "AppEvaluationCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppEvaluationInternalNoteDrafts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvaluationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEvaluationInternalNoteDrafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppEvaluationInternalNoteDrafts_AppEvaluations_EvaluationId",
                        column: x => x.EvaluationId,
                        principalTable: "AppEvaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppEvaluationInternalNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvaluationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEvaluationInternalNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppEvaluationInternalNotes_AppEvaluations_EvaluationId",
                        column: x => x.EvaluationId,
                        principalTable: "AppEvaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppEvaluationResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvaluationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Decision = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEvaluationResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppEvaluationResponses_AppEvaluations_EvaluationId",
                        column: x => x.EvaluationId,
                        principalTable: "AppEvaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppEvaluationScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvaluationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriterionId = table.Column<Guid>(type: "uuid", nullable: false),
                    GradeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEvaluationScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppEvaluationScores_AppEvaluations_EvaluationId",
                        column: x => x.EvaluationId,
                        principalTable: "AppEvaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppCircularRecipients_CircularId_UserId",
                table: "AppCircularRecipients",
                columns: new[] { "CircularId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppCircularRecipients_EmployeeId",
                table: "AppCircularRecipients",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCircularRecipients_UserId_AcknowledgedAt",
                table: "AppCircularRecipients",
                columns: new[] { "UserId", "AcknowledgedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppCirculars_SenderUserId_SentAt",
                table: "AppCirculars",
                columns: new[] { "SenderUserId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppDepartmentReports_CycleId_ManagerUserId",
                table: "AppDepartmentReports",
                columns: new[] { "CycleId", "ManagerUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppDepartmentReports_DepartmentId",
                table: "AppDepartmentReports",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppDepartmentReports_ManagerUserId",
                table: "AppDepartmentReports",
                column: "ManagerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppEvaluationCriteria_TenantId_NameAr",
                table: "AppEvaluationCriteria",
                columns: new[] { "TenantId", "NameAr" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppEvaluationCriteria_TenantId_NameEn",
                table: "AppEvaluationCriteria",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppEvaluationCycleCriteria_CycleId_CriterionId",
                table: "AppEvaluationCycleCriteria",
                columns: new[] { "CycleId", "CriterionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppEvaluationCycleGrades_CycleId_Code",
                table: "AppEvaluationCycleGrades",
                columns: new[] { "CycleId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppEvaluationCycles_TenantId_Status",
                table: "AppEvaluationCycles",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppEvaluationCycles_TenantId_Year_Month",
                table: "AppEvaluationCycles",
                columns: new[] { "TenantId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppEvaluationGrades_TenantId_Code",
                table: "AppEvaluationGrades",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppEvaluationInternalNoteDrafts_EvaluationId_AuthorUserId",
                table: "AppEvaluationInternalNoteDrafts",
                columns: new[] { "EvaluationId", "AuthorUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppEvaluationInternalNotes_EvaluationId_AuthorUserId",
                table: "AppEvaluationInternalNotes",
                columns: new[] { "EvaluationId", "AuthorUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppEvaluationInternalNotes_EvaluationId_CreationTime",
                table: "AppEvaluationInternalNotes",
                columns: new[] { "EvaluationId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppEvaluationResponses_EvaluationId_CreationTime",
                table: "AppEvaluationResponses",
                columns: new[] { "EvaluationId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppEvaluations_CycleId_EmployeeId",
                table: "AppEvaluations",
                columns: new[] { "CycleId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppEvaluations_CycleId_Status",
                table: "AppEvaluations",
                columns: new[] { "CycleId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppEvaluations_EmployeeId_Status",
                table: "AppEvaluations",
                columns: new[] { "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppEvaluations_ObjectionStatus",
                table: "AppEvaluations",
                column: "ObjectionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_AppEvaluationScores_EvaluationId_CriterionId",
                table: "AppEvaluationScores",
                columns: new[] { "EvaluationId", "CriterionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppCircularRecipients");

            migrationBuilder.DropTable(
                name: "AppDepartmentReports");

            migrationBuilder.DropTable(
                name: "AppEvaluationCriteria");

            migrationBuilder.DropTable(
                name: "AppEvaluationCycleCriteria");

            migrationBuilder.DropTable(
                name: "AppEvaluationCycleGrades");

            migrationBuilder.DropTable(
                name: "AppEvaluationGrades");

            migrationBuilder.DropTable(
                name: "AppEvaluationInternalNoteDrafts");

            migrationBuilder.DropTable(
                name: "AppEvaluationInternalNotes");

            migrationBuilder.DropTable(
                name: "AppEvaluationResponses");

            migrationBuilder.DropTable(
                name: "AppEvaluationScores");

            migrationBuilder.DropTable(
                name: "AppCirculars");

            migrationBuilder.DropTable(
                name: "AppEvaluations");

            migrationBuilder.DropTable(
                name: "AppEvaluationCycles");
        }
    }
}
