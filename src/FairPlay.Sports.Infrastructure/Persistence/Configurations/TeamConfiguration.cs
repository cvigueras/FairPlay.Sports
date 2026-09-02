using FairPlay.Sports.Domain.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FairPlay.Sports.Infrastructure.Persistence.Configurations;

internal sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");

        builder.HasKey(team => team.Id);
        builder.Property(team => team.Id).ValueGeneratedNever();
        builder.Property(team => team.Name).IsRequired().HasMaxLength(100);
        builder.Property(team => team.Coach).IsRequired().HasMaxLength(100);
        builder.Property(team => team.City).IsRequired().HasMaxLength(100);
        builder.Property(team => team.Type)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();
        builder.Property(team => team.Division)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();
        builder.Property(team => team.Category)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();
        builder.Property(team => team.Crest).HasColumnType("varbinary(max)");
        builder.Property(team => team.CrestContentType).HasMaxLength(100);
        builder.Property(team => team.CreatedAt).IsRequired();
        builder.Property(team => team.Active).IsRequired();

        builder.HasIndex(team => team.Name).IsUnique();
    }
}
