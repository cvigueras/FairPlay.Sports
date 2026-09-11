using FairPlay.Sports.Domain.Standings;
using FairPlay.Sports.Domain.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FairPlay.Sports.Infrastructure.Persistence.Configurations;

internal sealed class StandingConfiguration : IEntityTypeConfiguration<Standing>
{
    public void Configure(EntityTypeBuilder<Standing> builder)
    {
        builder.ToTable("Standings");

        builder.HasKey(standing => standing.Id);
        builder.Property(standing => standing.Id).ValueGeneratedNever();
        builder.Property(standing => standing.TeamId).IsRequired();
        builder.Property(standing => standing.Points).IsRequired();
        builder.Property(standing => standing.Played).IsRequired();
        builder.Property(standing => standing.Won).IsRequired();
        builder.Property(standing => standing.Drawn).IsRequired();
        builder.Property(standing => standing.Lost).IsRequired();
        builder.Property(standing => standing.GoalsFor).IsRequired();
        builder.Property(standing => standing.GoalsAgainst).IsRequired();
        builder.Property(standing => standing.CreatedAt).IsRequired();

        builder.HasIndex(standing => standing.TeamId).IsUnique();

        // FK for referential integrity only - no navigation property, the aggregates stay
        // referenced by id. A team with a standing cannot be deleted.
        builder.HasOne<Team>()
            .WithMany()
            .HasForeignKey(standing => standing.TeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
