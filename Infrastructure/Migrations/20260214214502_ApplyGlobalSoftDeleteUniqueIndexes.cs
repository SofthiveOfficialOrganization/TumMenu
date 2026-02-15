using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ApplyGlobalSoftDeleteUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // CLEANUP DUPLICATES BEFORE APPLYING INDEXES
            
            // Companies Slug
            migrationBuilder.Sql(@"
                WITH CTE AS (
                    SELECT Id, Slug, 
                           ROW_NUMBER() OVER (PARTITION BY Slug ORDER BY CreatedAt DESC) AS rn
                    FROM Companies
                    WHERE IsDeleted = 0
                )
                UPDATE Companies SET IsDeleted = 1, Slug = Slug + '-deleted-' + CAST(Id AS NVARCHAR(36)) 
                WHERE Id IN (SELECT Id FROM CTE WHERE rn > 1);
            ");

            // Companies OwnerId (Non-null)
            migrationBuilder.Sql(@"
                WITH CTE AS (
                    SELECT Id, OwnerId, 
                           ROW_NUMBER() OVER (PARTITION BY OwnerId ORDER BY CreatedAt DESC) AS rn
                    FROM Companies
                    WHERE IsDeleted = 0 AND OwnerId IS NOT NULL
                )
                UPDATE Companies SET IsDeleted = 1, Slug = Slug + '-deleted-owner-' + CAST(Id AS NVARCHAR(36))
                WHERE Id IN (SELECT Id FROM CTE WHERE rn > 1);
            ");

            // Store Slug
            migrationBuilder.Sql(@"
                WITH CTE AS (
                    SELECT Id, Slug, 
                           ROW_NUMBER() OVER (PARTITION BY Slug ORDER BY CreatedAt DESC) AS rn
                    FROM Stores
                    WHERE IsDeleted = 0
                )
                UPDATE Stores SET IsDeleted = 1 
                WHERE Id IN (SELECT Id FROM CTE WHERE rn > 1);
            ");

            // Product Slug
            migrationBuilder.Sql(@"
                WITH CTE AS (
                    SELECT Id, CategoryId, Slug, 
                           ROW_NUMBER() OVER (PARTITION BY CategoryId, Slug ORDER BY CreatedAt DESC) AS rn
                    FROM Products
                    WHERE IsDeleted = 0
                )
                UPDATE Products SET IsDeleted = 1 
                WHERE Id IN (SELECT Id FROM CTE WHERE rn > 1);
            ");

            // CategoryLibraryItems Slug
            migrationBuilder.Sql(@"
                WITH CTE AS (
                    SELECT Id, Slug, 
                           ROW_NUMBER() OVER (PARTITION BY Slug ORDER BY CreatedAt DESC) AS rn
                    FROM CategoryLibraryItems
                    WHERE IsDeleted = 0
                )
                UPDATE CategoryLibraryItems SET IsDeleted = 1 
                WHERE Id IN (SELECT Id FROM CTE WHERE rn > 1);
            ");

            // AdSlot Key
            migrationBuilder.Sql(@"
                WITH CTE AS (
                    SELECT Id, [Key], 
                           ROW_NUMBER() OVER (PARTITION BY [Key] ORDER BY CreatedAt DESC) AS rn
                    FROM AdSlots
                    WHERE IsDeleted = 0
                )
                UPDATE AdSlots SET IsDeleted = 1 
                WHERE Id IN (SELECT Id FROM CTE WHERE rn > 1);
            ");

            // QRCode PublicKey
            migrationBuilder.Sql(@"
                WITH CTE AS (
                    SELECT Id, PublicKey, 
                           ROW_NUMBER() OVER (PARTITION BY PublicKey ORDER BY CreatedAt DESC) AS rn
                    FROM QRCodes
                    WHERE IsDeleted = 0
                )
                UPDATE QRCodes SET IsDeleted = 1 
                WHERE Id IN (SELECT Id FROM CTE WHERE rn > 1);
            ");

            // UsageCounter (CompanyId, Key, Period)
            migrationBuilder.Sql(@"
                WITH CTE AS (
                    SELECT Id, CompanyId, [Key], Period,
                           ROW_NUMBER() OVER (PARTITION BY CompanyId, [Key], Period ORDER BY Id DESC) AS rn
                    FROM UsageCounters
                    WHERE IsDeleted = 0
                )
                DELETE FROM UsageCounters
                WHERE Id IN (SELECT Id FROM CTE WHERE rn > 1);
            ");

            // QRDailyStats (QRCodeId, Day)
            migrationBuilder.Sql(@"
                WITH CTE AS (
                    SELECT Id, QRCodeId, [Day],
                           ROW_NUMBER() OVER (PARTITION BY QRCodeId, [Day] ORDER BY Id DESC) AS rn
                    FROM QRDailyStats
                    WHERE IsDeleted = 0
                )
                DELETE FROM QRDailyStats
                WHERE Id IN (SELECT Id FROM CTE WHERE rn > 1);
            ");

            // PlanFeatures (PlanId, Key)
            migrationBuilder.Sql(@"
                WITH CTE AS (
                    SELECT Id, PlanId, [Key],
                           ROW_NUMBER() OVER (PARTITION BY PlanId, [Key] ORDER BY Id DESC) AS rn
                    FROM PlanFeatures
                    WHERE IsDeleted = 0
                )
                DELETE FROM PlanFeatures
                WHERE Id IN (SELECT Id FROM CTE WHERE rn > 1);
            ");

            migrationBuilder.DropIndex(
                name: "IX_UsageCounters_CompanyId_Key_Period",
                table: "UsageCounters");

            migrationBuilder.DropIndex(
                name: "IX_Stores_CompanyId_Slug",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Stores_Slug",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_QRDailyStats_QRCodeId_Day",
                table: "QRDailyStats");

            migrationBuilder.DropIndex(
                name: "IX_QRCodes_PublicKey",
                table: "QRCodes");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId_Slug",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_PlanFeatures_PlanId_Key",
                table: "PlanFeatures");

            migrationBuilder.DropIndex(
                name: "IX_Companies_OwnerId",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_CategoryLibraryItems_Slug",
                table: "CategoryLibraryItems");

            migrationBuilder.DropIndex(
                name: "IX_AdSlots_Key",
                table: "AdSlots");

            migrationBuilder.CreateIndex(
                name: "IX_UsageCounters_CompanyId_Key_Period",
                table: "UsageCounters",
                columns: new[] { "CompanyId", "Key", "Period" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_CompanyId_Slug",
                table: "Stores",
                columns: new[] { "CompanyId", "Slug" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_Slug",
                table: "Stores",
                column: "Slug",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_QRDailyStats_QRCodeId_Day",
                table: "QRDailyStats",
                columns: new[] { "QRCodeId", "Day" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_QRCodes_PublicKey",
                table: "QRCodes",
                column: "PublicKey",
                unique: true,
                filter: "[IsDeleted] = 0 AND [PublicKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId_Slug",
                table: "Products",
                columns: new[] { "CategoryId", "Slug" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PlanFeatures_PlanId_Key",
                table: "PlanFeatures",
                columns: new[] { "PlanId", "Key" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_OwnerId_Active",
                table: "Companies",
                column: "OwnerId",
                unique: true,
                filter: "[IsDeleted] = 0 AND [OwnerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Slug",
                table: "Companies",
                column: "Slug",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryLibraryItems_Slug",
                table: "CategoryLibraryItems",
                column: "Slug",
                unique: true,
                filter: "[IsDeleted] = 0 AND [Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AdSlots_Key",
                table: "AdSlots",
                column: "Key",
                unique: true,
                filter: "[IsDeleted] = 0 AND [Key] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UsageCounters_CompanyId_Key_Period",
                table: "UsageCounters");

            migrationBuilder.DropIndex(
                name: "IX_Stores_CompanyId_Slug",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Stores_Slug",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_QRDailyStats_QRCodeId_Day",
                table: "QRDailyStats");

            migrationBuilder.DropIndex(
                name: "IX_QRCodes_PublicKey",
                table: "QRCodes");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId_Slug",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_PlanFeatures_PlanId_Key",
                table: "PlanFeatures");

            migrationBuilder.DropIndex(
                name: "IX_Companies_OwnerId_Active",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_Slug",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_CategoryLibraryItems_Slug",
                table: "CategoryLibraryItems");

            migrationBuilder.DropIndex(
                name: "IX_AdSlots_Key",
                table: "AdSlots");

            migrationBuilder.CreateIndex(
                name: "IX_UsageCounters_CompanyId_Key_Period",
                table: "UsageCounters",
                columns: new[] { "CompanyId", "Key", "Period" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_CompanyId_Slug",
                table: "Stores",
                columns: new[] { "CompanyId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_Slug",
                table: "Stores",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QRDailyStats_QRCodeId_Day",
                table: "QRDailyStats",
                columns: new[] { "QRCodeId", "Day" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QRCodes_PublicKey",
                table: "QRCodes",
                column: "PublicKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId_Slug",
                table: "Products",
                columns: new[] { "CategoryId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanFeatures_PlanId_Key",
                table: "PlanFeatures",
                columns: new[] { "PlanId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_OwnerId",
                table: "Companies",
                column: "OwnerId",
                unique: true,
                filter: "[OwnerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryLibraryItems_Slug",
                table: "CategoryLibraryItems",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdSlots_Key",
                table: "AdSlots",
                column: "Key",
                unique: true);
        }
    }
}
