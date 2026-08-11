using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEntrepriseSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Telephone",
                table: "Entreprises",
                newName: "NumeroFiscal");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Entreprises",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Adresse",
                table: "Entreprises",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Contact",
                table: "Entreprises",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Entreprises",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Entreprises",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Entreprises",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Abonnements_EntrepriseId",
                table: "Abonnements",
                column: "EntrepriseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Abonnements_Entreprises_EntrepriseId",
                table: "Abonnements",
                column: "EntrepriseId",
                principalTable: "Entreprises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Abonnements_Entreprises_EntrepriseId",
                table: "Abonnements");

            migrationBuilder.DropIndex(
                name: "IX_Abonnements_EntrepriseId",
                table: "Abonnements");

            migrationBuilder.DropColumn(
                name: "Adresse",
                table: "Entreprises");

            migrationBuilder.DropColumn(
                name: "Contact",
                table: "Entreprises");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Entreprises");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Entreprises");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Entreprises");

            migrationBuilder.RenameColumn(
                name: "NumeroFiscal",
                table: "Entreprises",
                newName: "Telephone");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Entreprises",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
