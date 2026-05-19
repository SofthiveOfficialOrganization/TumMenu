using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuDesignTypographyAndStyleFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BodyFontSize",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ButtonBorderColor",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ButtonBorderRadius",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ButtonTextColor",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardBackgroundColor",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardBorderWidth",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeadingBorderColor",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeadingBorderWidth",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeadingFontWeight",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeadingTextColor",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkColor",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriceColor",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BodyFontSize",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "ButtonBorderColor",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "ButtonBorderRadius",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "ButtonTextColor",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "CardBackgroundColor",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "CardBorderWidth",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "HeadingBorderColor",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "HeadingBorderWidth",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "HeadingFontWeight",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "HeadingTextColor",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "LinkColor",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "PriceColor",
                table: "MenuDesigns");
        }
    }
}
