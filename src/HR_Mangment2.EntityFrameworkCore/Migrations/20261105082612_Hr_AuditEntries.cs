using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_Mangment2.Migrations
{
    /// <inheritdoc />
    public partial class Hr_AuditEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppAuditEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EntityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ChangedFields = table.Column<string[]>(type: "text[]", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Succeeded = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAuditEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAuditEntries_TenantId_Action_OccurredAt",
                table: "AppAuditEntries",
                columns: new[] { "TenantId", "Action", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAuditEntries_TenantId_EntityType_EntityId",
                table: "AppAuditEntries",
                columns: new[] { "TenantId", "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAuditEntries_TenantId_OccurredAt",
                table: "AppAuditEntries",
                columns: new[] { "TenantId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAuditEntries_TenantId_UserId_OccurredAt",
                table: "AppAuditEntries",
                columns: new[] { "TenantId", "UserId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAuditEntries");
        }
    }
}
