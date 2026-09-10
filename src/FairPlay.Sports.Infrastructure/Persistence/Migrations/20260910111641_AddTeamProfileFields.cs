using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FairPlay.Sports.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ColorPrimary",
                table: "Teams",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorSecondary",
                table: "Teams",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Teams",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Teams",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FoundedYear",
                table: "Teams",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortName",
                table: "Teams",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VenueAddress",
                table: "Teams",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VenueMapsUrl",
                table: "Teams",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VenueName",
                table: "Teams",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VenueSurface",
                table: "Teams",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Teams",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorPrimary",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "ColorSecondary",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "FoundedYear",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "ShortName",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "VenueAddress",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "VenueMapsUrl",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "VenueName",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "VenueSurface",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Teams");
        }
    }
}
