using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuDesignExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BackgroundImageUrl",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ButtonStyle",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CardBorderColor",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardImageBorderRadius",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardShadow",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FontFamily",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FooterBackgroundColor",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FooterButtonColor",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FooterButtonStyle",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FooterTextColor",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeaderBackgroundColor",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeaderTextColor",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeadingFontFamily",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeadingFontSize",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubheadingFontSize",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackgroundImageUrl",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "ButtonStyle",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "CardBorderColor",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "CardImageBorderRadius",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "CardShadow",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "FontFamily",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "FooterBackgroundColor",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "FooterButtonColor",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "FooterButtonStyle",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "FooterTextColor",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "HeaderBackgroundColor",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "HeaderTextColor",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "HeadingFontFamily",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "HeadingFontSize",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "SubheadingFontSize",
                table: "MenuDesigns");
        }
    }
}
