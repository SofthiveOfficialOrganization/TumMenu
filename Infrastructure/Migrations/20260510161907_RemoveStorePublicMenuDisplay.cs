using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStorePublicMenuDisplay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicMenuDisplay",
                table: "Stores");

            migrationBuilder.AddColumn<bool>(
                name: "ShowInSearchAndListings",
                table: "Stores",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowMenuButton",
                table: "Stores",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowPricesOnMenu",
                table: "Stores",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowInSearchAndListings",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "ShowMenuButton",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "ShowPricesOnMenu",
                table: "Stores");

            migrationBuilder.AddColumn<int>(
                name: "PublicMenuDisplay",
                table: "Stores",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
