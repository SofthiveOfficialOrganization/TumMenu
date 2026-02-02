using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryLibraryItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stores_CompanyId",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Slug",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Companies_Slug",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Tags",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Stores",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Products",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Plans",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Menus",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ExtensionPacks",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Companies",
                newName: "Title");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryLibraryItemId",
                table: "Categories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CategoryLibraryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IconKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryLibraryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryLibraryItems_CategoryLibraryItems_ParentId",
                        column: x => x.ParentId,
                        principalTable: "CategoryLibraryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CategoryLibraryItems_Medias_MediaId",
                        column: x => x.MediaId,
                        principalTable: "Medias",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stores_CompanyId_Slug",
                table: "Stores",
                columns: new[] { "CompanyId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId_Slug",
                table: "Products",
                columns: new[] { "CategoryId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CategoryLibraryItemId",
                table: "Categories",
                column: "CategoryLibraryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryLibraryItems_MediaId",
                table: "CategoryLibraryItems",
                column: "MediaId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryLibraryItems_ParentId",
                table: "CategoryLibraryItems",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryLibraryItems_Slug",
                table: "CategoryLibraryItems",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_CategoryLibraryItems_CategoryLibraryItemId",
                table: "Categories",
                column: "CategoryLibraryItemId",
                principalTable: "CategoryLibraryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_CategoryLibraryItems_CategoryLibraryItemId",
                table: "Categories");

            migrationBuilder.DropTable(
                name: "CategoryLibraryItems");

            migrationBuilder.DropIndex(
                name: "IX_Stores_CompanyId_Slug",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId_Slug",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Categories_CategoryLibraryItemId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CategoryLibraryItemId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Tags",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Stores",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Products",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Plans",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Menus",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "ExtensionPacks",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Companies",
                newName: "Name");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_CompanyId",
                table: "Stores",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug",
                table: "Products",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Slug",
                table: "Companies",
                column: "Slug",
                unique: true);
        }
    }
}
