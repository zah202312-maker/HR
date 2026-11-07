using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_Mangment2.Migrations
{
    /// <inheritdoc />
    public partial class Hr_Requests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppRequestTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Destination = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRequestTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Destination = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsOnBehalf = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DetailsSummaryAr = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    DetailsSummaryEn = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    SourceMemoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    RejectionReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CurrentApproverRole = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CurrentApproverUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentStepOrder = table.Column<int>(type: "integer", nullable: true),
                    CurrentStepSince = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastReminderAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppRequests_AppEmployees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "AppEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppRequests_AppRequestTypes_RequestTypeId",
                        column: x => x.RequestTypeId,
                        principalTable: "AppRequestTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppRequestSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Stage = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ApproverRole = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ApproverUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Decision = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DelegatedFromUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRequestSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppRequestSteps_AppRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "AppRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppRequestTimelineEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRequestTimelineEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppRequestTimelineEntries_AppRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "AppRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppRequests_CurrentApproverUserId_CurrentApproverRole",
                table: "AppRequests",
                columns: new[] { "CurrentApproverUserId", "CurrentApproverRole" });

            migrationBuilder.CreateIndex(
                name: "IX_AppRequests_EmployeeId_Status",
                table: "AppRequests",
                columns: new[] { "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppRequests_OwnerUserId_CreationTime",
                table: "AppRequests",
                columns: new[] { "OwnerUserId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppRequests_RequestTypeId",
                table: "AppRequests",
                column: "RequestTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRequests_Status_CreationTime",
                table: "AppRequests",
                columns: new[] { "Status", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppRequestSteps_RequestId_Order",
                table: "AppRequestSteps",
                columns: new[] { "RequestId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppRequestTimelineEntries_RequestId_OccurredAt",
                table: "AppRequestTimelineEntries",
                columns: new[] { "RequestId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppRequestTypes_IsActive",
                table: "AppRequestTypes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AppRequestTypes_TenantId_Code",
                table: "AppRequestTypes",
                columns: new[] { "TenantId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppRequestSteps");

            migrationBuilder.DropTable(
                name: "AppRequestTimelineEntries");

            migrationBuilder.DropTable(
                name: "AppRequests");

            migrationBuilder.DropTable(
                name: "AppRequestTypes");
        }
    }
}
