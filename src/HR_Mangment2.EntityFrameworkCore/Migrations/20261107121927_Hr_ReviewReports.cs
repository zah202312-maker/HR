using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_Mangment2.Migrations
{
    /// <inheritdoc />
    public partial class Hr_ReviewReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppReviewReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    PeriodFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodTo = table.Column<DateOnly>(type: "date", nullable: false),
                    Summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ViewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppReviewReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppReviewReportTeams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManagerEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    MembersCount = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CompletionPercent = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ProjectNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppReviewReportTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppReviewReportTeams_AppEmployees_ManagerEmployeeId",
                        column: x => x.ManagerEmployeeId,
                        principalTable: "AppEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppReviewReportTeams_AppReviewReports_ReviewReportId",
                        column: x => x.ReviewReportId,
                        principalTable: "AppReviewReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewReports_ReviewerUserId_PeriodFrom",
                table: "AppReviewReports",
                columns: new[] { "ReviewerUserId", "PeriodFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewReports_Status_SubmittedAt",
                table: "AppReviewReports",
                columns: new[] { "Status", "SubmittedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewReportTeams_ManagerEmployeeId",
                table: "AppReviewReportTeams",
                column: "ManagerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewReportTeams_ReviewReportId_ManagerEmployeeId",
                table: "AppReviewReportTeams",
                columns: new[] { "ReviewReportId", "ManagerEmployeeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppReviewReportTeams");

            migrationBuilder.DropTable(
                name: "AppReviewReports");
        }
    }
}
