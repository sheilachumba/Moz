using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moz.Migrations
{
    /// <inheritdoc />
    public partial class AddAddressAndIncomeProofTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressProofType",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncomeProofType",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressProofType",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "IncomeProofType",
                table: "IndividualKycs");
        }
    }
}
