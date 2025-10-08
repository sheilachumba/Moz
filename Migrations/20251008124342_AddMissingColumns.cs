using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moz.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Street",
                table: "IndividualKycs",
                newName: "WorkPhysical");

            migrationBuilder.RenameColumn(
                name: "Province",
                table: "IndividualKycs",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "Profession",
                table: "IndividualKycs",
                newName: "Subsegments");

            migrationBuilder.RenameColumn(
                name: "EmployerAddress",
                table: "IndividualKycs",
                newName: "State");

            migrationBuilder.RenameColumn(
                name: "District",
                table: "IndividualKycs",
                newName: "SourceOfFunds");

            migrationBuilder.RenameColumn(
                name: "Country",
                table: "IndividualKycs",
                newName: "SlaProcess");

            migrationBuilder.AddColumn<bool>(
                name: "AcceptMarketingCommunication",
                table: "IndividualKycs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AcceptRenewalEmail",
                table: "IndividualKycs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AcceptRenewalSms",
                table: "IndividualKycs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AddressVerification",
                table: "IndividualKycs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "IndividualKycs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceLcy",
                table: "IndividualKycs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BillToCustomerNo",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Blocked",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BlockingDate",
                table: "IndividualKycs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BvnNo",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Classification",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryRegionCode",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyFilter",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerCategory",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerPostingGroup",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerStatus",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DataProtectionConsent",
                table: "IndividualKycs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Email2",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GlobalDimension1Filter",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GlobalDimension2Filter",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HighRiskLowRisk",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IdExpiryDate",
                table: "IndividualKycs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IntermediaryName",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IntermediaryNo",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "KycFlag",
                table: "IndividualKycs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NextOfKinAddress",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextOfKinDob",
                table: "IndividualKycs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NextOfKinEmail",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NextOfKinGender",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NextOfKinName",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NextOfKinPhoneNo",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NextOfKinRelationship",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NextOfKinTitle",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "No",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OdataEtag",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OfficersName",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OnboardingDate",
                table: "IndividualKycs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PepStatus",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNo",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhysicalAddress",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostCode",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalAddress",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredBankAccountCode",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryEmail",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryPhoneNo",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PrivacyBlocked",
                table: "IndividualKycs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RelationshipManager",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelationshipManagerName",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReligionCode",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReligionName",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalesforceId",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Segments",
                table: "IndividualKycs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UnblockingDate",
                table: "IndividualKycs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UtilityBill",
                table: "IndividualKycs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptMarketingCommunication",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "AcceptRenewalEmail",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "AcceptRenewalSms",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "AddressVerification",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "Balance",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "BalanceLcy",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "BillToCustomerNo",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "Blocked",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "BlockingDate",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "BvnNo",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "City",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "Classification",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "CountryRegionCode",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "CurrencyFilter",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "CustomerCategory",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "CustomerPostingGroup",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "CustomerStatus",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "DataProtectionConsent",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "Email2",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "GlobalDimension1Filter",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "GlobalDimension2Filter",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "HighRiskLowRisk",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "IdExpiryDate",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "IntermediaryName",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "IntermediaryNo",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "KycFlag",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "NextOfKinAddress",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "NextOfKinDob",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "NextOfKinEmail",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "NextOfKinGender",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "NextOfKinName",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "NextOfKinPhoneNo",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "NextOfKinRelationship",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "NextOfKinTitle",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "No",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "Occupation",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "OdataEtag",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "OfficersName",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "OnboardingDate",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "PepStatus",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "PhoneNo",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "PhysicalAddress",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "PostCode",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "PostalAddress",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "PreferredBankAccountCode",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "PrimaryEmail",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "PrimaryPhoneNo",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "PrivacyBlocked",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "RelationshipManager",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "RelationshipManagerName",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "ReligionCode",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "ReligionName",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "SalesforceId",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "Segments",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "UnblockingDate",
                table: "IndividualKycs");

            migrationBuilder.DropColumn(
                name: "UtilityBill",
                table: "IndividualKycs");

            migrationBuilder.RenameColumn(
                name: "WorkPhysical",
                table: "IndividualKycs",
                newName: "Street");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "IndividualKycs",
                newName: "Province");

            migrationBuilder.RenameColumn(
                name: "Subsegments",
                table: "IndividualKycs",
                newName: "Profession");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "IndividualKycs",
                newName: "EmployerAddress");

            migrationBuilder.RenameColumn(
                name: "SourceOfFunds",
                table: "IndividualKycs",
                newName: "District");

            migrationBuilder.RenameColumn(
                name: "SlaProcess",
                table: "IndividualKycs",
                newName: "Country");
        }
    }
}
