using FairPlay.Sports.Domain.Challenges;
using FairPlay.Sports.Domain.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FairPlay.Sports.Infrastructure.Persistence.Configurations;

internal sealed class ChallengeConfiguration : IEntityTypeConfiguration<Challenge>
{
    public void Configure(EntityTypeBuilder<Challenge> builder)
    {
        builder.ToTable("Challenges");

        builder.HasKey(challenge => challenge.Id);
        builder.Property(challenge => challenge.Id).ValueGeneratedNever();
        builder.Property(challenge => challenge.MatchDate).IsRequired();
        builder.Property(challenge => challenge.AwayKitSlot)
            .HasMaxLength(10)
            .HasConversion<string>();
        builder.Property(challenge => challenge.Message).HasMaxLength(Challenge.MaxMessageLength);
        builder.Property(challenge => challenge.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();
        builder.Property(challenge => challenge.CreatedAt).IsRequired();
        builder.Ignore(challenge => challenge.HomeTeamId);
        builder.Ignore(challenge => challenge.AwayTeamId);
        builder.HasOne<Team>()
            .WithMany()
            .HasForeignKey(challenge => challenge.ChallengerTeamId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Team>()
            .WithMany()
            .HasForeignKey(challenge => challenge.ChallengedTeamId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Team>()
            .WithMany()
            .HasForeignKey(challenge => challenge.VenueTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(challenge => challenge.ChallengerTeamId);
        builder.HasIndex(challenge => challenge.ChallengedTeamId);
        builder.HasIndex(challenge => challenge.VenueTeamId);
    }
}
