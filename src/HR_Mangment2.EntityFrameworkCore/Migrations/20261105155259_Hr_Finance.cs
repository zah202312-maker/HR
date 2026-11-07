using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_Mangment2.Migrations
{
    /// <inheritdoc />
    public partial class Hr_Finance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "finance");

            migrationBuilder.CreateTable(
                name: "Adjustments",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Period = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Type = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Source = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    AttachmentFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AttachmentContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AttachmentSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    AttachmentBlobName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SourceRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    SuggestionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReversalOfAdjustmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adjustments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdjustmentSuggestions",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Period = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Type = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    Basis = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    QuantityUnit = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    SuggestedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    SettlementForPeriod = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    SourceCorrectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    AdjustmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdjustmentSuggestions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Source = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    AttachmentFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AttachmentContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AttachmentSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    AttachmentBlobName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    StockMovementId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReversalOfExpenseId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollRuns",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Period = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Status = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    PreparedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovalRequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastCalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsStale = table.Column<bool>(type: "boolean", nullable: false),
                    LastReturnReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EmployeesWithoutProfile = table.Column<string>(type: "text", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalaryProfiles",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BaseSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollItems",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    PayrollRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Gross = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Additions = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Deductions = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Net = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollItems_PayrollRuns_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalSchema: "finance",
                        principalTable: "PayrollRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalaryComponents",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalaryProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryComponents_SalaryProfiles_SalaryProfileId",
                        column: x => x.SalaryProfileId,
                        principalSchema: "finance",
                        principalTable: "SalaryProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollLines",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    PayrollItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Label = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AdjustmentId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollLines_PayrollItems_PayrollItemId",
                        column: x => x.PayrollItemId,
                        principalSchema: "finance",
                        principalTable: "PayrollItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Adjustments_EmployeeId_Period",
                schema: "finance",
                table: "Adjustments",
                columns: new[] { "EmployeeId", "Period" });

            migrationBuilder.CreateIndex(
                name: "IX_Adjustments_Period_Status",
                schema: "finance",
                table: "Adjustments",
                columns: new[] { "Period", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Adjustments_SourceRequestId",
                schema: "finance",
                table: "Adjustments",
                column: "SourceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_AdjustmentSuggestions_Period_Status",
                schema: "finance",
                table: "AdjustmentSuggestions",
                columns: new[] { "Period", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AdjustmentSuggestions_TenantId_EmployeeId_Period_Basis",
                schema: "finance",
                table: "AdjustmentSuggestions",
                columns: new[] { "TenantId", "EmployeeId", "Period", "Basis" },
                unique: true,
                filter: "\"SourceCorrectionId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AdjustmentSuggestions_TenantId_SourceCorrectionId",
                schema: "finance",
                table: "AdjustmentSuggestions",
                columns: new[] { "TenantId", "SourceCorrectionId" },
                unique: true,
                filter: "\"SourceCorrectionId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_Source_Date",
                schema: "finance",
                table: "Expenses",
                columns: new[] { "Source", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_SourceRequestId",
                schema: "finance",
                table: "Expenses",
                column: "SourceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_StockMovementId",
                schema: "finance",
                table: "Expenses",
                column: "StockMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollItems_EmployeeId",
                schema: "finance",
                table: "PayrollItems",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollItems_PayrollRunId_EmployeeId",
                schema: "finance",
                table: "PayrollItems",
                columns: new[] { "PayrollRunId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollLines_PayrollItemId",
                schema: "finance",
                table: "PayrollLines",
                column: "PayrollItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRuns_TenantId_Period",
                schema: "finance",
                table: "PayrollRuns",
                columns: new[] { "TenantId", "Period" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalaryComponents_SalaryProfileId",
                schema: "finance",
                table: "SalaryComponents",
                column: "SalaryProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryProfiles_TenantId_EmployeeId_EffectiveFrom",
                schema: "finance",
                table: "SalaryProfiles",
                columns: new[] { "TenantId", "EmployeeId", "EffectiveFrom" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Adjustments",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "AdjustmentSuggestions",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "Expenses",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "PayrollLines",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "SalaryComponents",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "PayrollItems",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "SalaryProfiles",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "PayrollRuns",
                schema: "finance");
        }
    }
}
