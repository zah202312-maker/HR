using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_Mangment2.Migrations
{
    /// <inheritdoc />
    public partial class Hr_ApprovedDecisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppEmployees_EmployeeNumber",
                table: "AppEmployees");

            migrationBuilder.CreateTable(
                name: "AppEmployeeHireDateCorrections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    OldHireDate = table.Column<DateOnly>(type: "date", nullable: false),
                    NewHireDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CorrectedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEmployeeHireDateCorrections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppEmployeeHireDateCorrections_AppEmployees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "AppEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployees_TenantId_EmployeeNumber",
                table: "AppEmployees",
                columns: new[] { "TenantId", "EmployeeNumber" },
                unique: true,
                filter: "\"EmployeeNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployeeHireDateCorrections_EmployeeId",
                table: "AppEmployeeHireDateCorrections",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppEmployeeHireDateCorrections");

            migrationBuilder.DropIndex(
                name: "IX_AppEmployees_TenantId_EmployeeNumber",
                table: "AppEmployees");

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployees_EmployeeNumber",
                table: "AppEmployees",
                column: "EmployeeNumber");
        }
    }
}
