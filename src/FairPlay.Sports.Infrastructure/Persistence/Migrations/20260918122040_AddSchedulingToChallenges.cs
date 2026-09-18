using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FairPlay.Sports.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSchedulingToChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AwayKitSlot",
                table: "Challenges",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "MatchDate",
                table: "Challenges",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "VenueTeamId",
                table: "Challenges",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_VenueTeamId",
                table: "Challenges",
                column: "VenueTeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Challenges_Teams_VenueTeamId",
                table: "Challenges",
                column: "VenueTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Challenges_Teams_VenueTeamId",
                table: "Challenges");

            migrationBuilder.DropIndex(
                name: "IX_Challenges_VenueTeamId",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "AwayKitSlot",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "MatchDate",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "VenueTeamId",
                table: "Challenges");
        }
    }
}
