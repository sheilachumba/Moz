using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moz.Migrations
{
    public partial class AddIndividualKycFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Profession",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdentityType",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdentityNumber",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "IdentityExpiryDate",
                table: "IndividualKycs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NextOfKinPhone",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SelectedSalutation",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "Profession",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "IdentityType",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "IdentityNumber",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "IdentityExpiryDate",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "NextOfKinPhone",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "SelectedSalutation",
                table: "IndividualKycs");
        }
    }
}
