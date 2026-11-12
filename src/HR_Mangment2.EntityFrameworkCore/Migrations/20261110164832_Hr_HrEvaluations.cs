using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_Mangment2.Migrations
{
    /// <inheritdoc />
    public partial class Hr_HrEvaluations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppHrEvaluationCriteria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_AppHrEvaluationCriteria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppHrEvaluationReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    SentByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EvaluationsCount = table.Column<int>(type: "integer", nullable: false),
                    FirstEvaluatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastEvaluatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHrEvaluationReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppHrEvaluations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ReportId = table.Column<Guid>(type: "uuid", nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHrEvaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHrEvaluations_AppEmployees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "AppEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppHrEvaluations_AppHrEvaluationReports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "AppHrEvaluationReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppHrEvaluationScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    HrEvaluationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriterionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHrEvaluationScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHrEvaluationScores_AppHrEvaluationCriteria_CriterionId",
                        column: x => x.CriterionId,
                        principalTable: "AppHrEvaluationCriteria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppHrEvaluationScores_AppHrEvaluations_HrEvaluationId",
                        column: x => x.HrEvaluationId,
                        principalTable: "AppHrEvaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppHrEvaluationCriteria_IsActive_SortOrder",
                table: "AppHrEvaluationCriteria",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHrEvaluationReports_SentByUserId_SentAt",
                table: "AppHrEvaluationReports",
                columns: new[] { "SentByUserId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHrEvaluations_EmployeeId_EvaluatedAt",
                table: "AppHrEvaluations",
                columns: new[] { "EmployeeId", "EvaluatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHrEvaluations_EvaluatedByUserId_ReportId",
                table: "AppHrEvaluations",
                columns: new[] { "EvaluatedByUserId", "ReportId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppHrEvaluations_ReportId",
                table: "AppHrEvaluations",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHrEvaluationScores_CriterionId",
                table: "AppHrEvaluationScores",
                column: "CriterionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHrEvaluationScores_HrEvaluationId_CriterionId",
                table: "AppHrEvaluationScores",
                columns: new[] { "HrEvaluationId", "CriterionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppHrEvaluationScores");

            migrationBuilder.DropTable(
                name: "AppHrEvaluationCriteria");

            migrationBuilder.DropTable(
                name: "AppHrEvaluations");

            migrationBuilder.DropTable(
                name: "AppHrEvaluationReports");
        }
    }
}
