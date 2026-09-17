using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FairPlay.Sports.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ScopeTeamNameUniquenessToClassification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teams_Name",
                table: "Teams");

            // A team's name only needs to be unique within its own modality, division and
            // category - the same name can be reused across classifications, and teams sharing
            // a name within one classification are told apart with a suffix (Real Madrid A / B).
            // EF Core can't express an index over a complex type's members via HasIndex, so it's
            // added directly on the underlying columns here instead.
            migrationBuilder.CreateIndex(
                name: "IX_Teams_Name_Type_Division_Category",
                table: "Teams",
                columns: ["Name", "Type", "Division", "Category"],
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teams_Name_Type_Division_Category",
                table: "Teams");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Name",
                table: "Teams",
                column: "Name",
                unique: true);
        }
    }
}
