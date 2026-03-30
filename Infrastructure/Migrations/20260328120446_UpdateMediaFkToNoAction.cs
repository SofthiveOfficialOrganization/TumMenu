using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMediaFkToNoAction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "CategoryLibraryItemId",
                table: "Medias",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medias_CategoryLibraryItemId",
                table: "Medias",
                column: "CategoryLibraryItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_CategoryLibraryItems_CategoryLibraryItemId",
                table: "Medias",
                column: "CategoryLibraryItemId",
                principalTable: "CategoryLibraryItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_CategoryLibraryItems_ReferenceId",
                table: "Medias",
                column: "ReferenceId",
                principalTable: "CategoryLibraryItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_Companies_ReferenceId",
                table: "Medias",
                column: "ReferenceId",
                principalTable: "Companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_Menus_ReferenceId",
                table: "Medias",
                column: "ReferenceId",
                principalTable: "Menus",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_Products_ReferenceId",
                table: "Medias",
                column: "ReferenceId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_Stores_ReferenceId",
                table: "Medias",
                column: "ReferenceId",
                principalTable: "Stores",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QRCodes_Stores_StoreId",
                table: "QRCodes",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medias_CategoryLibraryItems_CategoryLibraryItemId",
                table: "Medias");

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

            migrationBuilder.DropIndex(
                name: "IX_Medias_CategoryLibraryItemId",
                table: "Medias");

            migrationBuilder.DropColumn(
                name: "CategoryLibraryItemId",
                table: "Medias");

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_CategoryLibraryItems_ReferenceId",
                table: "Medias",
                column: "ReferenceId",
                principalTable: "CategoryLibraryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_Companies_ReferenceId",
                table: "Medias",
                column: "ReferenceId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_Menus_ReferenceId",
                table: "Medias",
                column: "ReferenceId",
                principalTable: "Menus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_Products_ReferenceId",
                table: "Medias",
                column: "ReferenceId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_Stores_ReferenceId",
                table: "Medias",
                column: "ReferenceId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QRCodes_Stores_StoreId",
                table: "QRCodes",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
