using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FairPlay.Sports.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamVenueAvailable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "VenueAvailable",
                table: "Teams",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VenueAvailable",
                table: "Teams");
        }
    }
}
