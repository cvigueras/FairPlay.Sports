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

        // Optional "ficha" fields. Loose scalars first.
        builder.Property(team => team.ShortName).HasMaxLength(Team.MaxShortNameLength);
        builder.Property(team => team.FoundedYear);
        builder.Property(team => team.ContactEmail).HasMaxLength(Team.MaxContactEmailLength);
        builder.Property(team => team.ContactPhone).HasMaxLength(Team.MaxContactPhoneLength);
        builder.Property(team => team.Website).HasMaxLength(Team.MaxWebsiteLength);

        // Optional value objects: owned (not complex) so the whole instance can be
        // null. All columns are nullable; an absent venue / kit is all-null.
        builder.OwnsOne(team => team.HomeVenue, venue =>
        {
            venue.Property(v => v.Name).HasColumnName("VenueName").HasMaxLength(Venue.MaxNameLength);
            venue.Property(v => v.Address).HasColumnName("VenueAddress").HasMaxLength(Venue.MaxAddressLength);
            venue.Property(v => v.Surface)
                .HasColumnName("VenueSurface")
                .HasMaxLength(20)
                .HasConversion<string>();
            venue.Property(v => v.MapsUrl).HasColumnName("VenueMapsUrl").HasMaxLength(Venue.MaxMapsUrlLength);
        });
        builder.Navigation(team => team.HomeVenue).IsRequired(false);

        builder.OwnsOne(team => team.Colors, colors =>
        {
            colors.Property(c => c.Primary).HasColumnName("ColorPrimary").HasMaxLength(KitColors.MaxColourLength);
            colors.Property(c => c.Secondary).HasColumnName("ColorSecondary").HasMaxLength(KitColors.MaxColourLength);
        });
        builder.Navigation(team => team.Colors).IsRequired(false);

        // byte[] maps to PostgreSQL 'bytea' by convention - no explicit column type needed.
        builder.Property(team => team.CrestContentType).HasMaxLength(100);
        builder.Property(team => team.CreatedAt).IsRequired();
        builder.Property(team => team.Active).IsRequired();

        builder.HasIndex(team => team.Name).IsUnique();
    }
}
