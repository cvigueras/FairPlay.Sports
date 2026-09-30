using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FairPlay.Sports.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamAcceptsChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AcceptsChallenges",
                table: "Teams",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptsChallenges",
                table: "Teams");
        }
    }
}
