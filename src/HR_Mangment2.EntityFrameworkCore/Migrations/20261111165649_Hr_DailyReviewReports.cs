using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_Mangment2.Migrations
{
    /// <inheritdoc />
    public partial class Hr_DailyReviewReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The period follow-up reports are replaced by the daily report, and their data goes (decision 2026-11-11).
            migrationBuilder.Sql("DELETE FROM \"AppNotifications\" WHERE \"TemplateCode\" = 'ReviewReportSubmitted';");
            migrationBuilder.Sql("DELETE FROM \"AppReviewReportTeams\";");
            migrationBuilder.Sql("DELETE FROM \"AppReviewReports\";");

            migrationBuilder.DropIndex(
                name: "IX_AppReviewReportTeams_ReviewReportId_ManagerEmployeeId",
                table: "AppReviewReportTeams");

            migrationBuilder.DropIndex(
                name: "IX_AppReviewReports_ReviewerUserId_PeriodFrom",
                table: "AppReviewReports");

            migrationBuilder.DropColumn(
                name: "CompletionPercent",
                table: "AppReviewReportTeams");

            migrationBuilder.DropColumn(
                name: "MembersCount",
                table: "AppReviewReportTeams");

            migrationBuilder.DropColumn(
                name: "PeriodFrom",
                table: "AppReviewReports");

            migrationBuilder.DropColumn(
                name: "PeriodType",
                table: "AppReviewReports");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "AppReviewReports");

            migrationBuilder.RenameColumn(
                name: "ProjectNotes",
                table: "AppReviewReportTeams",
                newName: "FrontendNotes");

            migrationBuilder.RenameColumn(
                name: "PeriodTo",
                table: "AppReviewReports",
                newName: "ReportDate");

            migrationBuilder.AddColumn<string>(
                name: "BackendGrade",
                table: "AppReviewReportTeams",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BackendNotes",
                table: "AppReviewReportTeams",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FrontendGrade",
                table: "AppReviewReportTeams",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<List<Guid>>(
                name: "MemberEmployeeIds",
                table: "AppReviewReportTeams",
                type: "uuid[]",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "RoomId",
                table: "AppReviewReportTeams",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "RoomName",
                table: "AppReviewReportTeams",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "AppReviewReports",
                type: "character varying(10000)",
                maxLength: 10000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppReviewRooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ManagerEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberEmployeeIds = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
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
                    table.PrimaryKey("PK_AppReviewRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppReviewRooms_AppEmployees_ManagerEmployeeId",
                        column: x => x.ManagerEmployeeId,
                        principalTable: "AppEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewReportTeams_ReviewReportId_RoomId",
                table: "AppReviewReportTeams",
                columns: new[] { "ReviewReportId", "RoomId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewReports_ReviewerUserId_ReportDate",
                table: "AppReviewReports",
                columns: new[] { "ReviewerUserId", "ReportDate" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewRooms_ManagerEmployeeId",
                table: "AppReviewRooms",
                column: "ManagerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewRooms_ReviewerUserId_ManagerEmployeeId",
                table: "AppReviewRooms",
                columns: new[] { "ReviewerUserId", "ManagerEmployeeId" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppReviewRooms");

            migrationBuilder.DropIndex(
                name: "IX_AppReviewReportTeams_ReviewReportId_RoomId",
                table: "AppReviewReportTeams");

            migrationBuilder.DropIndex(
                name: "IX_AppReviewReports_ReviewerUserId_ReportDate",
                table: "AppReviewReports");

            migrationBuilder.DropColumn(
                name: "BackendGrade",
                table: "AppReviewReportTeams");

            migrationBuilder.DropColumn(
                name: "BackendNotes",
                table: "AppReviewReportTeams");

            migrationBuilder.DropColumn(
                name: "FrontendGrade",
                table: "AppReviewReportTeams");

            migrationBuilder.DropColumn(
                name: "MemberEmployeeIds",
                table: "AppReviewReportTeams");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "AppReviewReportTeams");

            migrationBuilder.DropColumn(
                name: "RoomName",
                table: "AppReviewReportTeams");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "AppReviewReports");

            migrationBuilder.RenameColumn(
                name: "FrontendNotes",
                table: "AppReviewReportTeams",
                newName: "ProjectNotes");

            migrationBuilder.RenameColumn(
                name: "ReportDate",
                table: "AppReviewReports",
                newName: "PeriodTo");

            migrationBuilder.AddColumn<int>(
                name: "CompletionPercent",
                table: "AppReviewReportTeams",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MembersCount",
                table: "AppReviewReportTeams",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PeriodFrom",
                table: "AppReviewReports",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "PeriodType",
                table: "AppReviewReports",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "AppReviewReports",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewReportTeams_ReviewReportId_ManagerEmployeeId",
                table: "AppReviewReportTeams",
                columns: new[] { "ReviewReportId", "ManagerEmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewReports_ReviewerUserId_PeriodFrom",
                table: "AppReviewReports",
                columns: new[] { "ReviewerUserId", "PeriodFrom" });
        }
    }
}
