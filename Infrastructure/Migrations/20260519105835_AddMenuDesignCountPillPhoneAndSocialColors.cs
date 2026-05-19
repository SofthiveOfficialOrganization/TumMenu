using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuDesignCountPillPhoneAndSocialColors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountPillBackgroundColor",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountPillBorderColor",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountPillOpacity",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountPillTextColor",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneLinkColor",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SocialIconBackgroundColor",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SocialIconBorderColor",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SocialIconColor",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SocialIconOpacity",
                table: "MenuDesigns",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountPillBackgroundColor",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "CountPillBorderColor",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "CountPillOpacity",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "CountPillTextColor",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "PhoneLinkColor",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "SocialIconBackgroundColor",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "SocialIconBorderColor",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "SocialIconColor",
                table: "MenuDesigns");

            migrationBuilder.DropColumn(
                name: "SocialIconOpacity",
                table: "MenuDesigns");
        }
    }
}
