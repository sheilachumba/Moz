using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MOZ_UPGRADE.Migrations
{
    public partial class AddEmailVerificationTimestamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationTimestamp",
                table: "Users",
                type: "datetime(6)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerificationTimestamp",
                table: "Users");
        }
    }
}
