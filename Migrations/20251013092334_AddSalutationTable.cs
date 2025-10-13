using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moz.Migrations
{
    /// <inheritdoc />
    public partial class AddSalutationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Means_of_ID",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "Means_of_ID",
                table: "IndividualKycs");
        }
    }
}
