using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_Mangment2.Migrations
{
    /// <inheritdoc />
    public partial class Hr_EmploymentRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppAppreciationLetters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IssuedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FileId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    BlobName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAppreciationLetters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAppreciationLetters_AppEmployees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "AppEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PreviousContractId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    EmployeePosition = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Recommendation = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    RecommendationNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RecommendedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecommendedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionDueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Decision = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    DecisionNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EmployeeMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EmployeeNotifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiringNotifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionDueNotifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppContracts_AppContracts_PreviousContractId",
                        column: x => x.PreviousContractId,
                        principalTable: "AppContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppContracts_AppEmployees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "AppEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppWarnings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecommendedLevel = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    IncidentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    RecommendedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecommenderRole = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    NotifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResponseDueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ResponseContent = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ResponseSubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionOutcome = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    IssuedLevel = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    DecisionNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ObjectionDueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ObjectionContent = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ObjectionSubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ObjectionOutcome = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    ObjectionResolutionNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ObjectionResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ObjectionResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppWarnings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppWarnings_AppEmployees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "AppEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppWarningAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    WarningId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    BlobName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppWarningAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppWarningAttachments_AppWarnings_WarningId",
                        column: x => x.WarningId,
                        principalTable: "AppWarnings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAppreciationLetters_EmployeeId_IssuedAt",
                table: "AppAppreciationLetters",
                columns: new[] { "EmployeeId", "IssuedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppContracts_EmployeeId_StartDate",
                table: "AppContracts",
                columns: new[] { "EmployeeId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppContracts_PreviousContractId",
                table: "AppContracts",
                column: "PreviousContractId");

            migrationBuilder.CreateIndex(
                name: "IX_AppContracts_Status_EndDate",
                table: "AppContracts",
                columns: new[] { "Status", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppWarningAttachments_WarningId",
                table: "AppWarningAttachments",
                column: "WarningId");

            migrationBuilder.CreateIndex(
                name: "IX_AppWarnings_EmployeeId_Status",
                table: "AppWarnings",
                columns: new[] { "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppWarnings_RecommendedByUserId_CreationTime",
                table: "AppWarnings",
                columns: new[] { "RecommendedByUserId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppWarnings_Status_ResponseDueDate",
                table: "AppWarnings",
                columns: new[] { "Status", "ResponseDueDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAppreciationLetters");

            migrationBuilder.DropTable(
                name: "AppContracts");

            migrationBuilder.DropTable(
                name: "AppWarningAttachments");

            migrationBuilder.DropTable(
                name: "AppWarnings");
        }
    }
}
