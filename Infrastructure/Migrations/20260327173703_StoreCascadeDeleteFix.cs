using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StoreCascadeDeleteFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Stores_StoreId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_CategoryLibraryItems_Medias_MediaId",
                table: "CategoryLibraryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Medias_Companies_CompanyId",
                table: "Medias");

            migrationBuilder.DropForeignKey(
                name: "FK_Medias_Menus_MenuId",
                table: "Medias");

            migrationBuilder.DropForeignKey(
                name: "FK_Medias_Products_ProductId",
                table: "Medias");

            migrationBuilder.DropForeignKey(
                name: "FK_Medias_Stores_StoreId",
                table: "Medias");

            migrationBuilder.DropForeignKey(
                name: "FK_QRCodes_Stores_StoreId",
                table: "QRCodes");

            migrationBuilder.DropIndex(
                name: "IX_Medias_CompanyId",
                table: "Medias");

            migrationBuilder.DropIndex(
                name: "IX_Medias_MenuId",
                table: "Medias");

            migrationBuilder.DropIndex(
                name: "IX_Medias_ProductId",
                table: "Medias");

            migrationBuilder.DropIndex(
                name: "IX_Medias_StoreId",
                table: "Medias");

            migrationBuilder.DropIndex(
                name: "IX_CategoryLibraryItems_MediaId",
                table: "CategoryLibraryItems");

            migrationBuilder.DropColumn(
                name: "MenuId",
                table: "Medias");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Medias");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Medias");

            migrationBuilder.DropColumn(
                name: "MediaId",
                table: "CategoryLibraryItems");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Stores_StoreId",
                table: "Addresses",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_CategoryLibraryItems_ReferenceId",
                table: "Medias",
                column: "ReferenceId",
                principalTable: "CategoryLibraryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_Companies_ReferenceId",
                table: "Medias",
                column: "ReferenceId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_Menus_ReferenceId",
                table: "Medias",
                column: "ReferenceId",
                principalTable: "Menus",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_Products_ReferenceId",
                table: "Medias",
                column: "ReferenceId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_Stores_ReferenceId",
                table: "Medias",
                column: "ReferenceId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_QRCodes_Stores_StoreId",
                table: "QRCodes",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Stores_StoreId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Medias_CategoryLibraryItems_ReferenceId",
                table: "Medias");

            migrationBuilder.DropForeignKey(
                name: "FK_Medias_Companies_ReferenceId",
                table: "Medias");

            migrationBuilder.DropForeignKey(
                name: "FK_Medias_Menus_ReferenceId",
                table: "Medias");

            migrationBuilder.DropForeignKey(
                name: "FK_Medias_Products_ReferenceId",
                table: "Medias");

            migrationBuilder.DropForeignKey(
                name: "FK_Medias_Stores_ReferenceId",
                table: "Medias");

            migrationBuilder.DropForeignKey(
                name: "FK_QRCodes_Stores_StoreId",
                table: "QRCodes");

            migrationBuilder.AddColumn<Guid>(
                name: "MenuId",
                table: "Medias",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "Medias",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Medias",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MediaId",
                table: "CategoryLibraryItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medias_CompanyId",
                table: "Medias",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Medias_MenuId",
                table: "Medias",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_Medias_ProductId",
                table: "Medias",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Medias_StoreId",
                table: "Medias",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryLibraryItems_MediaId",
                table: "CategoryLibraryItems",
                column: "MediaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Stores_StoreId",
                table: "Addresses",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryLibraryItems_Medias_MediaId",
                table: "CategoryLibraryItems",
                column: "MediaId",
                principalTable: "Medias",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_Companies_CompanyId",
                table: "Medias",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_Menus_MenuId",
                table: "Medias",
                column: "MenuId",
                principalTable: "Menus",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_Products_ProductId",
                table: "Medias",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_Stores_StoreId",
                table: "Medias",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QRCodes_Stores_StoreId",
                table: "QRCodes",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id");
        }
    }
}
