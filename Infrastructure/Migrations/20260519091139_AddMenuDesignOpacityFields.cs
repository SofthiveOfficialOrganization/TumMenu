using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuDesignOpacityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardOpacity",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeaderAccentTextOpacity",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeaderChipOpacity",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeaderSecondaryTextOpacity",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MutedTextOpacity",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PageBackgroundOverlayOpacity",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PanelOpacity",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SurfaceOpacity",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardOpacity",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "HeaderAccentTextOpacity",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "HeaderChipOpacity",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "HeaderSecondaryTextOpacity",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "MutedTextOpacity",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "PageBackgroundOverlayOpacity",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "PanelOpacity",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "SurfaceOpacity",
                table: "MenuDesigns");
        }
    }
}
