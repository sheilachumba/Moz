using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MOZ_UPGRADE.Migrations
{
    public partial class Add2FAFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTwoFAEnabled",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TwoFACode",
                table: "Users",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "TwoFACodeExpiry",
                table: "Users",
                type: "datetime(6)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTwoFAEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TwoFACode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TwoFACodeExpiry",
                table: "Users");
        }
    }
}
