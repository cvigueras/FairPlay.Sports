using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FairPlay.Sports.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedNever();
        builder.Property(user => user.UserName).IsRequired().HasMaxLength(50);
        builder.Property(user => user.Email).IsRequired().HasMaxLength(256);
        builder.Property(user => user.PasswordHash).IsRequired().HasMaxLength(500);
        builder.Property(user => user.TeamId).IsRequired();
        builder.Property(user => user.Role)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(UserRole.Member);
        builder.Property(user => user.CreatedAt).IsRequired();
        builder.Property(user => user.Active).IsRequired();

        builder.HasIndex(user => user.Email).IsUnique();
        builder.HasIndex(user => user.UserName).IsUnique();
        builder.HasIndex(user => user.TeamId);

        // FK for referential integrity only - no navigation property, the aggregates stay
        // referenced by id. A team with members cannot be deleted.
        builder.HasOne<Team>()
            .WithMany()
            .HasForeignKey(user => user.TeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
