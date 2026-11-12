using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_Mangment2.Migrations
{
    /// <inheritdoc />
    public partial class Added_ReviewerEvaluations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppReviewerEvaluationCriteria",
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
                    table.PrimaryKey("PK_AppReviewerEvaluationCriteria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppReviewerEvaluationReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    SentByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EvaluationsCount = table.Column<int>(type: "integer", nullable: false),
                    FirstEvaluationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LastEvaluationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppReviewerEvaluationReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppReviewerEvaluations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluationDate = table.Column<DateOnly>(type: "date", nullable: false),
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
                    table.PrimaryKey("PK_AppReviewerEvaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppReviewerEvaluations_AppEmployees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "AppEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppReviewerEvaluations_AppReviewerEvaluationReports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "AppReviewerEvaluationReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppReviewerEvaluationItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewerEvaluationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriterionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Grade = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppReviewerEvaluationItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppReviewerEvaluationItems_AppReviewerEvaluationCriteria_Cr~",
                        column: x => x.CriterionId,
                        principalTable: "AppReviewerEvaluationCriteria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppReviewerEvaluationItems_AppReviewerEvaluations_ReviewerE~",
                        column: x => x.ReviewerEvaluationId,
                        principalTable: "AppReviewerEvaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewerEvaluationCriteria_IsActive_SortOrder",
                table: "AppReviewerEvaluationCriteria",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewerEvaluationItems_CriterionId",
                table: "AppReviewerEvaluationItems",
                column: "CriterionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewerEvaluationItems_ReviewerEvaluationId_CriterionId",
                table: "AppReviewerEvaluationItems",
                columns: new[] { "ReviewerEvaluationId", "CriterionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewerEvaluationReports_SentByUserId_SentAt",
                table: "AppReviewerEvaluationReports",
                columns: new[] { "SentByUserId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewerEvaluations_EmployeeId_EvaluationDate",
                table: "AppReviewerEvaluations",
                columns: new[] { "EmployeeId", "EvaluationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewerEvaluations_EvaluatedByUserId_ReportId",
                table: "AppReviewerEvaluations",
                columns: new[] { "EvaluatedByUserId", "ReportId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewerEvaluations_ReportId",
                table: "AppReviewerEvaluations",
                column: "ReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppReviewerEvaluationItems");

            migrationBuilder.DropTable(
                name: "AppReviewerEvaluationCriteria");

            migrationBuilder.DropTable(
                name: "AppReviewerEvaluations");

            migrationBuilder.DropTable(
                name: "AppReviewerEvaluationReports");
        }
    }
}
