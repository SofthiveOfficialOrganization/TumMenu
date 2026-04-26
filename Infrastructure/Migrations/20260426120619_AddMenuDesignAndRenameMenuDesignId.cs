using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuDesignAndRenameMenuDesignId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MenuTemplateId",
                table: "Menus",
                newName: "MenuDesignId");

            migrationBuilder.CreateTable(
                name: "MenuDesigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrimaryDarkColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccentColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BackgroundColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SurfaceColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TextColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MutedColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BorderRadius = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BackgroundGradient = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreviewImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuDesigns", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Menus_MenuDesignId",
                table: "Menus",
                column: "MenuDesignId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuDesigns_Slug",
                table: "MenuDesigns",
                column: "Slug",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_MenuDesigns_MenuDesignId",
                table: "Menus",
                column: "MenuDesignId",
                principalTable: "MenuDesigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Menus_MenuDesigns_MenuDesignId",
                table: "Menus");

            migrationBuilder.DropTable(
                name: "MenuDesigns");

            migrationBuilder.DropIndex(
                name: "IX_Menus_MenuDesignId",
                table: "Menus");

            migrationBuilder.RenameColumn(
                name: "MenuDesignId",
                table: "Menus",
                newName: "MenuTemplateId");
        }
    }
}
