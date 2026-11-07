using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_Mangment2.Migrations
{
    /// <inheritdoc />
    public partial class Hr_MemosAndIncidents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppIncidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmitterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    CurrentManagerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceMemoId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppIncidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppIncidents_AppEmployees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "AppEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppMemos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    WriterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    WriterRow = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    CurrentHandlerRole = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CurrentHandlerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CloseReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ConvertedRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConvertedIncidentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMemos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppMemos_AppEmployees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "AppEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppIncidentTimelineEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    IncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppIncidentTimelineEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppIncidentTimelineEntries_AppIncidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "AppIncidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppMemoHandlers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    MemoId = table.Column<Guid>(type: "uuid", nullable: false),
                    HandlerRole = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    HandlerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArrivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMemoHandlers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppMemoHandlers_AppMemos_MemoId",
                        column: x => x.MemoId,
                        principalTable: "AppMemos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppMemoTimelineEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    MemoId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMemoTimelineEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppMemoTimelineEntries_AppMemos_MemoId",
                        column: x => x.MemoId,
                        principalTable: "AppMemos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppIncidents_CurrentManagerUserId",
                table: "AppIncidents",
                column: "CurrentManagerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppIncidents_EmployeeId_Status",
                table: "AppIncidents",
                columns: new[] { "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppIncidents_Status_CreationTime",
                table: "AppIncidents",
                columns: new[] { "Status", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppIncidents_SubmitterUserId_CreationTime",
                table: "AppIncidents",
                columns: new[] { "SubmitterUserId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppIncidentTimelineEntries_IncidentId_OccurredAt",
                table: "AppIncidentTimelineEntries",
                columns: new[] { "IncidentId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppMemoHandlers_HandlerUserId_HandlerRole",
                table: "AppMemoHandlers",
                columns: new[] { "HandlerUserId", "HandlerRole" });

            migrationBuilder.CreateIndex(
                name: "IX_AppMemoHandlers_MemoId",
                table: "AppMemoHandlers",
                column: "MemoId");

            migrationBuilder.CreateIndex(
                name: "IX_AppMemos_CurrentHandlerUserId_CurrentHandlerRole",
                table: "AppMemos",
                columns: new[] { "CurrentHandlerUserId", "CurrentHandlerRole" });

            migrationBuilder.CreateIndex(
                name: "IX_AppMemos_EmployeeId_Status",
                table: "AppMemos",
                columns: new[] { "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppMemos_WriterUserId_CreationTime",
                table: "AppMemos",
                columns: new[] { "WriterUserId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppMemoTimelineEntries_MemoId_OccurredAt",
                table: "AppMemoTimelineEntries",
                columns: new[] { "MemoId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppIncidentTimelineEntries");

            migrationBuilder.DropTable(
                name: "AppMemoHandlers");

            migrationBuilder.DropTable(
                name: "AppMemoTimelineEntries");

            migrationBuilder.DropTable(
                name: "AppIncidents");

            migrationBuilder.DropTable(
                name: "AppMemos");
        }
    }
}
