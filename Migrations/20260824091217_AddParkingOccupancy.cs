using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class AddParkingOccupancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlacesDisponibles",
                table: "Parkings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlacesOccupees",
                table: "Parkings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "TauxOccupation",
                table: "Parkings",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlacesDisponibles",
                table: "Parkings");

            migrationBuilder.DropColumn(
                name: "PlacesOccupees",
                table: "Parkings");

            migrationBuilder.DropColumn(
                name: "TauxOccupation",
                table: "Parkings");
        }
    }
}
