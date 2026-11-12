using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_Mangment2.Migrations
{
    /// <inheritdoc />
    public partial class Added_HrEvaluationAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppHrEvaluationAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriterionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AverageScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    EvaluationsCount = table.Column<int>(type: "integer", nullable: false),
                    Threshold = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    PeriodFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodTo = table.Column<DateOnly>(type: "date", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHrEvaluationAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHrEvaluationAlerts_AppHrEvaluationCriteria_CriterionId",
                        column: x => x.CriterionId,
                        principalTable: "AppHrEvaluationCriteria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppHrEvaluationAlerts_CriterionId",
                table: "AppHrEvaluationAlerts",
                column: "CriterionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHrEvaluationAlerts_EmployeeId_CriterionId_PeriodTo",
                table: "AppHrEvaluationAlerts",
                columns: new[] { "EmployeeId", "CriterionId", "PeriodTo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppHrEvaluationAlerts_PeriodTo",
                table: "AppHrEvaluationAlerts",
                column: "PeriodTo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppHrEvaluationAlerts");
        }
    }
}
