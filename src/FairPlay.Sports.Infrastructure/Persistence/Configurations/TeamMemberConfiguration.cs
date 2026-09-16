using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FairPlay.Sports.Infrastructure.Persistence.Configurations;

internal sealed class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.ToTable("TeamMembers");

        builder.HasKey(member => member.Id);
        builder.Property(member => member.Id).ValueGeneratedNever();
        builder.Property(member => member.Role)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();
        builder.Property(member => member.DisplayName).IsRequired().HasMaxLength(TeamMember.MaxDisplayNameLength);
        builder.Property(member => member.CreatedAt).IsRequired();

        // FK-only for referential integrity - no navigation property, the aggregates
        // stay referenced by id. A team/user with memberships cannot be deleted.
        builder.HasOne<Team>()
            .WithMany()
            .HasForeignKey(member => member.TeamId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(member => member.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // One role per user per team.
        builder.HasIndex(member => new { member.TeamId, member.UserId }).IsUnique();

        // At most one Coach/President/TechnicalStaff/Delegate per team; Player is unrestricted.
        builder.HasIndex(member => new { member.TeamId, member.Role })
            .IsUnique()
            .HasFilter("\"Role\" <> 'Player'");
    }
}
