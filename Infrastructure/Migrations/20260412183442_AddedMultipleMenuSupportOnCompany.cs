using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedMultipleMenuSupportOnCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Menus_CompanyId",
                table: "Menus");

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultMainMenuId",
                table: "Companies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Menus_CompanyId",
                table: "Menus",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_DefaultMainMenuId",
                table: "Companies",
                column: "DefaultMainMenuId");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Menus_DefaultMainMenuId",
                table: "Companies",
                column: "DefaultMainMenuId",
                principalTable: "Menus",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Menus_DefaultMainMenuId",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Menus_CompanyId",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_Companies_DefaultMainMenuId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "DefaultMainMenuId",
                table: "Companies");

            migrationBuilder.CreateIndex(
                name: "IX_Menus_CompanyId",
                table: "Menus",
                column: "CompanyId",
                unique: true,
                filter: "[CompanyId] IS NOT NULL");
        }
    }
}
