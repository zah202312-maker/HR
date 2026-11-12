using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_Mangment2.Migrations
{
    /// <inheritdoc />
    public partial class Hr_StoreDevelopment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentNumber",
                table: "AppStockMovements",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IssueVoucherId",
                table: "AppStockMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseId",
                table: "AppStockMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "AppStockMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "AppStockMovements",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageUnitCost",
                table: "AppItems",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "AppItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "AppItems",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UnitId",
                table: "AppItems",
                type: "uuid",
                nullable: true);

            // Items from before 2026-11-11 get ITM-0001 and on, in creation order per company, before the unique index.
            migrationBuilder.Sql("""
                UPDATE "AppItems" AS i
                SET "Sku" = 'ITM-' || lpad(n.rn::text, 4, '0')
                FROM (
                    SELECT "Id", row_number() OVER (PARTITION BY "TenantId" ORDER BY "CreationTime", "Id") AS rn
                    FROM "AppItems"
                ) AS n
                WHERE i."Id" = n."Id";
                """);

            // Old receipts with a cost give each item its first average: their total cost over their quantity.
            migrationBuilder.Sql("""
                UPDATE "AppItems" AS i
                SET "AverageUnitCost" = round(c.cost / c.qty, 4)
                FROM (
                    SELECT "ItemId", sum("TotalCost") AS cost, sum("Quantity") AS qty
                    FROM "AppStockMovements"
                    WHERE "Type" = 'Receive' AND "TotalCost" IS NOT NULL AND "Quantity" > 0
                    GROUP BY "ItemId"
                ) AS c
                WHERE i."Id" = c."ItemId" AND c.qty > 0;
                """);

            migrationBuilder.CreateTable(
                name: "AppItemCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    NameAr = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppItemCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppItemUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    NameAr = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppItemUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppStoreDocumentAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    BlobName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStoreDocumentAttachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppStorePurchases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SupplierName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PurchaseDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ReceivedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReversedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReversedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReverseReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStorePurchases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppStoreTeams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_AppStoreTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStoreTeams_AppDepartments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "AppDepartments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppStorePurchaseLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sku = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    StockMovementId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStorePurchaseLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStorePurchaseLines_AppItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "AppItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppStorePurchaseLines_AppStorePurchases_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "AppStorePurchases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppStoreIssueVouchers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VoucherNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReversedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReversedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReverseReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStoreIssueVouchers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStoreIssueVouchers_AppEmployees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "AppEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppStoreIssueVouchers_AppStoreTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "AppStoreTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppStoreIssueVoucherLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    IssueVoucherId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sku = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    LineValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    CustodyId = table.Column<Guid>(type: "uuid", nullable: true),
                    StockMovementId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStoreIssueVoucherLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStoreIssueVoucherLines_AppCustodies_CustodyId",
                        column: x => x.CustodyId,
                        principalTable: "AppCustodies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppStoreIssueVoucherLines_AppItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "AppItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppStoreIssueVoucherLines_AppStoreIssueVouchers_IssueVouche~",
                        column: x => x.IssueVoucherId,
                        principalTable: "AppStoreIssueVouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppStockMovements_IssueVoucherId",
                table: "AppStockMovements",
                column: "IssueVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStockMovements_PurchaseId",
                table: "AppStockMovements",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStockMovements_TeamId_CreationTime",
                table: "AppStockMovements",
                columns: new[] { "TeamId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppStockMovements_TenantId_DocumentNumber",
                table: "AppStockMovements",
                columns: new[] { "TenantId", "DocumentNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_AppItems_CategoryId",
                table: "AppItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppItems_TenantId_Sku",
                table: "AppItems",
                columns: new[] { "TenantId", "Sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppItems_UnitId",
                table: "AppItems",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_AppItemCategories_TenantId_NameAr",
                table: "AppItemCategories",
                columns: new[] { "TenantId", "NameAr" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppItemCategories_TenantId_NameEn",
                table: "AppItemCategories",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppItemUnits_TenantId_NameAr",
                table: "AppItemUnits",
                columns: new[] { "TenantId", "NameAr" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppItemUnits_TenantId_NameEn",
                table: "AppItemUnits",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppStoreDocumentAttachments_DocumentId",
                table: "AppStoreDocumentAttachments",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStoreIssueVoucherLines_CustodyId",
                table: "AppStoreIssueVoucherLines",
                column: "CustodyId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStoreIssueVoucherLines_IssueVoucherId",
                table: "AppStoreIssueVoucherLines",
                column: "IssueVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStoreIssueVoucherLines_ItemId",
                table: "AppStoreIssueVoucherLines",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStoreIssueVouchers_EmployeeId",
                table: "AppStoreIssueVouchers",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStoreIssueVouchers_TeamId_IssueDate",
                table: "AppStoreIssueVouchers",
                columns: new[] { "TeamId", "IssueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppStoreIssueVouchers_TenantId_Status_IssueDate",
                table: "AppStoreIssueVouchers",
                columns: new[] { "TenantId", "Status", "IssueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppStoreIssueVouchers_TenantId_VoucherNumber",
                table: "AppStoreIssueVouchers",
                columns: new[] { "TenantId", "VoucherNumber" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AppStorePurchaseLines_ItemId",
                table: "AppStorePurchaseLines",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStorePurchaseLines_PurchaseId",
                table: "AppStorePurchaseLines",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStorePurchases_SupplierName",
                table: "AppStorePurchases",
                column: "SupplierName");

            migrationBuilder.CreateIndex(
                name: "IX_AppStorePurchases_TenantId_ReceiptNumber",
                table: "AppStorePurchases",
                columns: new[] { "TenantId", "ReceiptNumber" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AppStorePurchases_TenantId_Status_PurchaseDate",
                table: "AppStorePurchases",
                columns: new[] { "TenantId", "Status", "PurchaseDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppStoreTeams_DepartmentId",
                table: "AppStoreTeams",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStoreTeams_TenantId_Name",
                table: "AppStoreTeams",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppItems_AppItemCategories_CategoryId",
                table: "AppItems",
                column: "CategoryId",
                principalTable: "AppItemCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppItems_AppItemUnits_UnitId",
                table: "AppItems",
                column: "UnitId",
                principalTable: "AppItemUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppStockMovements_AppStoreIssueVouchers_IssueVoucherId",
                table: "AppStockMovements",
                column: "IssueVoucherId",
                principalTable: "AppStoreIssueVouchers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppStockMovements_AppStorePurchases_PurchaseId",
                table: "AppStockMovements",
                column: "PurchaseId",
                principalTable: "AppStorePurchases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppStockMovements_AppStoreTeams_TeamId",
                table: "AppStockMovements",
                column: "TeamId",
                principalTable: "AppStoreTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppItems_AppItemCategories_CategoryId",
                table: "AppItems");

            migrationBuilder.DropForeignKey(
                name: "FK_AppItems_AppItemUnits_UnitId",
                table: "AppItems");

            migrationBuilder.DropForeignKey(
                name: "FK_AppStockMovements_AppStoreIssueVouchers_IssueVoucherId",
                table: "AppStockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_AppStockMovements_AppStorePurchases_PurchaseId",
                table: "AppStockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_AppStockMovements_AppStoreTeams_TeamId",
                table: "AppStockMovements");

            migrationBuilder.DropTable(
                name: "AppItemCategories");

            migrationBuilder.DropTable(
                name: "AppItemUnits");

            migrationBuilder.DropTable(
                name: "AppStoreDocumentAttachments");

            migrationBuilder.DropTable(
                name: "AppStoreIssueVoucherLines");

            migrationBuilder.DropTable(
                name: "AppStorePurchaseLines");

            migrationBuilder.DropTable(
                name: "AppStoreIssueVouchers");

            migrationBuilder.DropTable(
                name: "AppStorePurchases");

            migrationBuilder.DropTable(
                name: "AppStoreTeams");

            migrationBuilder.DropIndex(
                name: "IX_AppStockMovements_IssueVoucherId",
                table: "AppStockMovements");

            migrationBuilder.DropIndex(
                name: "IX_AppStockMovements_PurchaseId",
                table: "AppStockMovements");

            migrationBuilder.DropIndex(
                name: "IX_AppStockMovements_TeamId_CreationTime",
                table: "AppStockMovements");

            migrationBuilder.DropIndex(
                name: "IX_AppStockMovements_TenantId_DocumentNumber",
                table: "AppStockMovements");

            migrationBuilder.DropIndex(
                name: "IX_AppItems_CategoryId",
                table: "AppItems");

            migrationBuilder.DropIndex(
                name: "IX_AppItems_TenantId_Sku",
                table: "AppItems");

            migrationBuilder.DropIndex(
                name: "IX_AppItems_UnitId",
                table: "AppItems");

            migrationBuilder.DropColumn(
                name: "DocumentNumber",
                table: "AppStockMovements");

            migrationBuilder.DropColumn(
                name: "IssueVoucherId",
                table: "AppStockMovements");

            migrationBuilder.DropColumn(
                name: "PurchaseId",
                table: "AppStockMovements");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "AppStockMovements");

            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "AppStockMovements");

            migrationBuilder.DropColumn(
                name: "AverageUnitCost",
                table: "AppItems");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "AppItems");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "AppItems");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "AppItems");
        }
    }
}
