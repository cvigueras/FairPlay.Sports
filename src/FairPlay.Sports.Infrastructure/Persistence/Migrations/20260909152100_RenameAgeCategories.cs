using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FairPlay.Sports.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameAgeCategories : Migration
    {
        // AgeCategory is stored as a string, so renaming the enum members is not a
        // schema change - only the existing "Teams"."Category" values need mapping.
        private static readonly (string Old, string New)[] Map =
        [
            ("Under6", "Chupetes"),
            ("Under8", "Prebenjamines"),
            ("Under10", "Benjamines"),
            ("Under12", "Alevines"),
            ("Under14", "Infantiles"),
            ("Under16", "Cadetes"),
            ("Under19", "Juveniles"),
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var (oldValue, newValue) in Map)
            {
                migrationBuilder.Sql(
                    $"""UPDATE "Teams" SET "Category" = '{newValue}' WHERE "Category" = '{oldValue}';""");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var (oldValue, newValue) in Map)
            {
                migrationBuilder.Sql(
                    $"""UPDATE "Teams" SET "Category" = '{oldValue}' WHERE "Category" = '{newValue}';""");
            }
        }
    }
}
