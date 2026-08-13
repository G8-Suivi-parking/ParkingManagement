using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ParkingManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAcces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Acces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParkingId = table.Column<int>(type: "integer", nullable: false),
                    ZoneId = table.Column<int>(type: "integer", nullable: false),
                    Immatriculation = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DateEntree = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateSortie = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstPresent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Acces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Acces_Parkings_ParkingId",
                        column: x => x.ParkingId,
                        principalTable: "Parkings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Acces_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Acces_ParkingId",
                table: "Acces",
                column: "ParkingId");

            migrationBuilder.CreateIndex(
                name: "IX_Acces_ZoneId",
                table: "Acces",
                column: "ZoneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Acces");
        }
    }
}
