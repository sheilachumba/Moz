using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MOZ_UPGRADE.Migrations
{
    public partial class AddCompanyRepresentativeToUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyRepresentative",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyRepresentative",
                table: "Users");
        }
    }
}
