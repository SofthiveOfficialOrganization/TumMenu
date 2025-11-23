using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updated_Owner_at_Company_to_nullable_temporarily : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Companies_OwnerId",
                table: "Companies");

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "Companies",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_OwnerId",
                table: "Companies",
                column: "OwnerId",
                unique: true,
                filter: "[OwnerId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Companies_OwnerId",
                table: "Companies");

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "Companies",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_OwnerId",
                table: "Companies",
                column: "OwnerId",
                unique: true);
        }
    }
}
