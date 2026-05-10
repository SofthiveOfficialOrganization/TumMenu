using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StorePublicDisplayAndSecondaryPhone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PublicMenuDisplay",
                table: "Stores",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryPhoneNumber",
                table: "Stores",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowRepresentativeImagesDisclaimer",
                table: "Stores",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicMenuDisplay",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "SecondaryPhoneNumber",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "ShowRepresentativeImagesDisclaimer",
                table: "Stores");
        }
    }
}
