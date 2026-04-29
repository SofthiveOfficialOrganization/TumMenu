using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCategoryLibraryHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoryLibraryItems_CategoryLibraryItems_ParentId",
                table: "CategoryLibraryItems");

            migrationBuilder.DropIndex(
                name: "IX_CategoryLibraryItems_ParentId",
                table: "CategoryLibraryItems");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "CategoryLibraryItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "CategoryLibraryItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoryLibraryItems_ParentId",
                table: "CategoryLibraryItems",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryLibraryItems_CategoryLibraryItems_ParentId",
                table: "CategoryLibraryItems",
                column: "ParentId",
                principalTable: "CategoryLibraryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
