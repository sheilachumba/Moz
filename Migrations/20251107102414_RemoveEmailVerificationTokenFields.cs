using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MOZ_UPGRADE.Migrations
{
    public partial class RemoveEmailVerificationTokenFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerificationToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailVerificationTokenExpiry",
                table: "Users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationToken",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationTokenExpiry",
                table: "Users",
                type: "datetime(6)",
                nullable: true);
        }
    }
}
