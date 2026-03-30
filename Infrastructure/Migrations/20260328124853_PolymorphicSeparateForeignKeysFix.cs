using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PolymorphicSeparateForeignKeysFix : Migration
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

            migrationBuilder.AlterColumn<Guid>(
                name: "ReferenceId",
                table: "Medias",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.DropColumn(
                name: "MenuId",
                table: "Medias");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Medias");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Medias");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReferenceId",
                table: "Medias",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

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
        }
    }
}
