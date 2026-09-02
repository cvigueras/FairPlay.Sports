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

        // Value object flattened onto the same columns (Type / Division / Category)
        // so the schema is unchanged.
        builder.ComplexProperty(team => team.Classification, classification =>
        {
            classification.Property(c => c.Type)
                .HasColumnName("Type")
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>();
            classification.Property(c => c.Division)
                .HasColumnName("Division")
                .IsRequired()
                .HasMaxLength(30)
                .HasConversion<string>();
            classification.Property(c => c.Category)
                .HasColumnName("Category")
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>();
        });

        builder.Property(team => team.Crest).HasColumnType("varbinary(max)");
        builder.Property(team => team.CrestContentType).HasMaxLength(100);
        builder.Property(team => team.CreatedAt).IsRequired();
        builder.Property(team => team.Active).IsRequired();

        builder.HasIndex(team => team.Name).IsUnique();
    }
}
