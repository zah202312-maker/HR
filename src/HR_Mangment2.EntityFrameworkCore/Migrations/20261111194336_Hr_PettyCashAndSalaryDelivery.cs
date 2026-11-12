using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_Mangment2.Migrations
{
    /// <inheritdoc />
    public partial class Hr_PettyCashAndSalaryDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeliveredByUserId",
                schema: "finance",
                table: "PayrollItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DeliveredOn",
                schema: "finance",
                table: "PayrollItems",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PettyCashTypes",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    NameAr = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
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
                    table.PrimaryKey("PK_PettyCashTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PettyCashAssignments",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    FromPeriod = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    ToPeriod = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PettyCashAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PettyCashAssignments_PettyCashTypes_TypeId",
                        column: x => x.TypeId,
                        principalSchema: "finance",
                        principalTable: "PettyCashTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PettyCashEntries",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Period = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    DeliveredOn = table.Column<DateOnly>(type: "date", nullable: true),
                    DeliveredByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpenseId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PettyCashEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PettyCashEntries_PettyCashAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalSchema: "finance",
                        principalTable: "PettyCashAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PettyCashAssignments_EmployeeId_TypeId",
                schema: "finance",
                table: "PettyCashAssignments",
                columns: new[] { "EmployeeId", "TypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_PettyCashAssignments_TypeId",
                schema: "finance",
                table: "PettyCashAssignments",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PettyCashEntries_AssignmentId_Period",
                schema: "finance",
                table: "PettyCashEntries",
                columns: new[] { "AssignmentId", "Period" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PettyCashEntries_TenantId_Period_Status",
                schema: "finance",
                table: "PettyCashEntries",
                columns: new[] { "TenantId", "Period", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PettyCashTypes_TenantId_NameAr",
                schema: "finance",
                table: "PettyCashTypes",
                columns: new[] { "TenantId", "NameAr" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PettyCashTypes_TenantId_NameEn",
                schema: "finance",
                table: "PettyCashTypes",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PettyCashEntries",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "PettyCashAssignments",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "PettyCashTypes",
                schema: "finance");

            migrationBuilder.DropColumn(
                name: "DeliveredByUserId",
                schema: "finance",
                table: "PayrollItems");

            migrationBuilder.DropColumn(
                name: "DeliveredOn",
                schema: "finance",
                table: "PayrollItems");
        }
    }
}
