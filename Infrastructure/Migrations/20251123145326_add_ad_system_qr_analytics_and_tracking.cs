using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class add_ad_system_qr_analytics_and_tracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Store_StoreId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_Store_StoreId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLine_Invoices_InvoiceId",
                table: "InvoiceLine");

            migrationBuilder.DropForeignKey(
                name: "FK_Menus_Store_StoreId",
                table: "Menus");

            migrationBuilder.DropForeignKey(
                name: "FK_QRCodes_Companies_CompanyId",
                table: "QRCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_QRCodes_Menus_MenuId",
                table: "QRCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_QRCodes_Store_StoreId",
                table: "QRCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Staffs_Store_StoreId",
                table: "Staffs");

            migrationBuilder.DropForeignKey(
                name: "FK_Store_Companies_CompanyId",
                table: "Store");

            migrationBuilder.DropIndex(
                name: "IX_QRCodes_CompanyId",
                table: "QRCodes");

            migrationBuilder.DropIndex(
                name: "IX_Menus_CompanyId",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_Menus_StoreId",
                table: "Menus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Store",
                table: "Store");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InvoiceLine",
                table: "InvoiceLine");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "QRCodes");

            migrationBuilder.DropColumn(
                name: "IsDynamic",
                table: "QRCodes");

            migrationBuilder.RenameTable(
                name: "Store",
                newName: "Stores");

            migrationBuilder.RenameTable(
                name: "InvoiceLine",
                newName: "InvoiceLines");

            migrationBuilder.RenameIndex(
                name: "IX_Store_Slug",
                table: "Stores",
                newName: "IX_Stores_Slug");

            migrationBuilder.RenameIndex(
                name: "IX_Store_CompanyId",
                table: "Stores",
                newName: "IX_Stores_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_InvoiceLine_InvoiceId",
                table: "InvoiceLines",
                newName: "IX_InvoiceLines_InvoiceId");

            migrationBuilder.AlterColumn<string>(
                name: "TargetUrl",
                table: "QRCodes",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2048)",
                oldMaxLength: 2048);

            migrationBuilder.AlterColumn<int>(
                name: "ECCLevel",
                table: "QRCodes",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "ResolveMode",
                table: "QRCodes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedPreparationTimeInMinutes",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Version",
                table: "Menus",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId",
                table: "Menus",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "CountryId",
                table: "Addresses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Stores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Stores",
                table: "Stores",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InvoiceLines",
                table: "InvoiceLines",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AdCreatives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClickUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdCreatives", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdRevenueImports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Network = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Day = table.Column<DateOnly>(type: "date", nullable: false),
                    Revenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceFile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdRevenueImports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdSlots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Entity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Ip = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Channel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TemplateKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScheduledAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ToUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PlanFeatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanFeatures_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QRDailyStats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QRCodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Day = table.Column<DateOnly>(type: "date", nullable: false),
                    Scans = table.Column<int>(type: "int", nullable: false),
                    UniqueIps = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRDailyStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QRDailyStats_QRCodes_QRCodeId",
                        column: x => x.QRCodeId,
                        principalTable: "QRCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QRScanEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QRCodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScannedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Referrer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRScanEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QRScanEvents_QRCodes_QRCodeId",
                        column: x => x.QRCodeId,
                        principalTable: "QRCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsageCounters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    Period = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageCounters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsageCounters_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebhookEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProviderEventId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdPlacements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdSlotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdCreativeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DailyCap = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdPlacements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdPlacements_AdCreatives_AdCreativeId",
                        column: x => x.AdCreativeId,
                        principalTable: "AdCreatives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdPlacements_AdSlots_AdSlotId",
                        column: x => x.AdSlotId,
                        principalTable: "AdSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdClicks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdPlacementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClickedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdClicks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdClicks_AdPlacements_AdPlacementId",
                        column: x => x.AdPlacementId,
                        principalTable: "AdPlacements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdImpressions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdPlacementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShownAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdImpressions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdImpressions_AdPlacements_AdPlacementId",
                        column: x => x.AdPlacementId,
                        principalTable: "AdPlacements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QRCodes_PublicKey",
                table: "QRCodes",
                column: "PublicKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Menus_CompanyId",
                table: "Menus",
                column: "CompanyId",
                unique: true,
                filter: "[CompanyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Menus_StoreId",
                table: "Menus",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_DefaultPaymentMethodId",
                table: "Companies",
                column: "DefaultPaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_AdClicks_AdPlacementId",
                table: "AdClicks",
                column: "AdPlacementId");

            migrationBuilder.CreateIndex(
                name: "IX_AdImpressions_AdPlacementId",
                table: "AdImpressions",
                column: "AdPlacementId");

            migrationBuilder.CreateIndex(
                name: "IX_AdPlacements_AdCreativeId",
                table: "AdPlacements",
                column: "AdCreativeId");

            migrationBuilder.CreateIndex(
                name: "IX_AdPlacements_AdSlotId",
                table: "AdPlacements",
                column: "AdSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_AdSlots_Key",
                table: "AdSlots",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CompanyId",
                table: "Notifications",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ToUserId",
                table: "Notifications",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanFeatures_PlanId_Key",
                table: "PlanFeatures",
                columns: new[] { "PlanId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QRDailyStats_QRCodeId_Day",
                table: "QRDailyStats",
                columns: new[] { "QRCodeId", "Day" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QRScanEvents_QRCodeId",
                table: "QRScanEvents",
                column: "QRCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_UsageCounters_CompanyId_Key_Period",
                table: "UsageCounters",
                columns: new[] { "CompanyId", "Key", "Period" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Stores_StoreId",
                table: "Addresses",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_PaymentMethods_DefaultPaymentMethodId",
                table: "Companies",
                column: "DefaultPaymentMethodId",
                principalTable: "PaymentMethods",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Stores_StoreId",
                table: "Images",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLines_Invoices_InvoiceId",
                table: "InvoiceLines",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_Stores_StoreId",
                table: "Menus",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QRCodes_Menus_MenuId",
                table: "QRCodes",
                column: "MenuId",
                principalTable: "Menus",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_QRCodes_Stores_StoreId",
                table: "QRCodes",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Staffs_Stores_StoreId",
                table: "Staffs",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_Companies_CompanyId",
                table: "Stores",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Stores_StoreId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Companies_PaymentMethods_DefaultPaymentMethodId",
                table: "Companies");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_Stores_StoreId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLines_Invoices_InvoiceId",
                table: "InvoiceLines");

            migrationBuilder.DropForeignKey(
                name: "FK_Menus_Stores_StoreId",
                table: "Menus");

            migrationBuilder.DropForeignKey(
                name: "FK_QRCodes_Menus_MenuId",
                table: "QRCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_QRCodes_Stores_StoreId",
                table: "QRCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Staffs_Stores_StoreId",
                table: "Staffs");

            migrationBuilder.DropForeignKey(
                name: "FK_Stores_Companies_CompanyId",
                table: "Stores");

            migrationBuilder.DropTable(
                name: "AdClicks");

            migrationBuilder.DropTable(
                name: "AdImpressions");

            migrationBuilder.DropTable(
                name: "AdRevenueImports");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PlanFeatures");

            migrationBuilder.DropTable(
                name: "QRDailyStats");

            migrationBuilder.DropTable(
                name: "QRScanEvents");

            migrationBuilder.DropTable(
                name: "UsageCounters");

            migrationBuilder.DropTable(
                name: "WebhookEvents");

            migrationBuilder.DropTable(
                name: "AdPlacements");

            migrationBuilder.DropTable(
                name: "AdCreatives");

            migrationBuilder.DropTable(
                name: "AdSlots");

            migrationBuilder.DropIndex(
                name: "IX_QRCodes_PublicKey",
                table: "QRCodes");

            migrationBuilder.DropIndex(
                name: "IX_Menus_CompanyId",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_Menus_StoreId",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_Companies_DefaultPaymentMethodId",
                table: "Companies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Stores",
                table: "Stores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InvoiceLines",
                table: "InvoiceLines");

            migrationBuilder.DropColumn(
                name: "ResolveMode",
                table: "QRCodes");

            migrationBuilder.DropColumn(
                name: "EstimatedPreparationTimeInMinutes",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Stores");

            migrationBuilder.RenameTable(
                name: "Stores",
                newName: "Store");

            migrationBuilder.RenameTable(
                name: "InvoiceLines",
                newName: "InvoiceLine");

            migrationBuilder.RenameIndex(
                name: "IX_Stores_Slug",
                table: "Store",
                newName: "IX_Store_Slug");

            migrationBuilder.RenameIndex(
                name: "IX_Stores_CompanyId",
                table: "Store",
                newName: "IX_Store_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_InvoiceLines_InvoiceId",
                table: "InvoiceLine",
                newName: "IX_InvoiceLine_InvoiceId");

            migrationBuilder.AlterColumn<string>(
                name: "TargetUrl",
                table: "QRCodes",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(2048)",
                oldMaxLength: 2048,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ECCLevel",
                table: "QRCodes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "QRCodes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDynamic",
                table: "QRCodes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "Menus",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId",
                table: "Menus",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Store",
                table: "Store",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InvoiceLine",
                table: "InvoiceLine",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_QRCodes_CompanyId",
                table: "QRCodes",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Menus_CompanyId",
                table: "Menus",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Menus_StoreId",
                table: "Menus",
                column: "StoreId",
                unique: true,
                filter: "[StoreId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Store_StoreId",
                table: "Addresses",
                column: "StoreId",
                principalTable: "Store",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Store_StoreId",
                table: "Images",
                column: "StoreId",
                principalTable: "Store",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLine_Invoices_InvoiceId",
                table: "InvoiceLine",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_Store_StoreId",
                table: "Menus",
                column: "StoreId",
                principalTable: "Store",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QRCodes_Companies_CompanyId",
                table: "QRCodes",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QRCodes_Menus_MenuId",
                table: "QRCodes",
                column: "MenuId",
                principalTable: "Menus",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QRCodes_Store_StoreId",
                table: "QRCodes",
                column: "StoreId",
                principalTable: "Store",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Staffs_Store_StoreId",
                table: "Staffs",
                column: "StoreId",
                principalTable: "Store",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Store_Companies_CompanyId",
                table: "Store",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
