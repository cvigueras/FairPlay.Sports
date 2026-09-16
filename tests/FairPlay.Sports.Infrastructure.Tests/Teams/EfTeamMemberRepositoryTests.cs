using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.Infrastructure.Persistence;
using FairPlay.Sports.Infrastructure.Teams;
using FairPlay.Sports.TestSupport.Teams;
using FairPlay.Sports.TestSupport.Users;
using Microsoft.EntityFrameworkCore;

namespace FairPlay.Sports.Infrastructure.Tests.Teams;

/// <summary>
/// Integration tests for <see cref="EfTeamMemberRepository"/> against a real PostgreSQL
/// (Testcontainers). They cover what an in-memory provider cannot: the two unique indexes
/// backing the membership invariants - one role per user per team, and at most one
/// Coach/President/TechnicalStaff/Delegate per team, while Player stays unrestricted.
/// </summary>
[TestFixture]
public class EfTeamMemberRepositoryTests : RepositoryTestBase
{
    // Members carry FKs to both Teams and Users; every seeded member references these.
    [SetUp]
    public async Task SeedReferencedTeamAndUsers()
    {
        await using var context = NewContext();
        context.Teams.Add(TeamMother.DomainTeam(id: TeamMemberMother.TeamId));
        context.Users.Add(UserMother.DomainUser(id: TeamMemberMother.UserId));
        context.Users.Add(UserMother.DomainUser(id: OtherUserId, userName: "other", email: "other@example.com"));
        await context.SaveChangesAsync();
    }

    private static readonly Guid OtherUserId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    [Test]
    public async Task AddAsync_thenCommit_persistsTheMember()
    {
        var member = TeamMemberMother.DomainTeamMember();

        await using (var arrange = NewContext())
        {
            var repository = new EfTeamMemberRepository(arrange);
            var unitOfWork = new UnitOfWork(arrange);
            await repository.AddAsync(member);
            await unitOfWork.SaveChangesAsync();
        }

        await using var assert = NewContext();
        var persisted = await assert.TeamMembers.AsNoTracking().SingleOrDefaultAsync(m => m.Id == member.Id);
        Assert.That(persisted, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(persisted!.TeamId, Is.EqualTo(member.TeamId));
            Assert.That(persisted.UserId, Is.EqualTo(member.UserId));
            Assert.That(persisted.Role, Is.EqualTo(member.Role));
            Assert.That(persisted.DisplayName, Is.EqualTo(member.DisplayName));
        });
    }

    [Test]
    public async Task Commit_withSameUserTwiceOnSameTeam_throwsDbUpdateException()
    {
        await using var context = NewContext();
        context.TeamMembers.Add(TeamMemberMother.DomainTeamMember(role: TeamMemberRole.Player));
        context.TeamMembers.Add(TeamMemberMother.DomainTeamMember(role: TeamMemberRole.Delegate));

        Assert.That(async () => await context.SaveChangesAsync(), Throws.InstanceOf<DbUpdateException>());
    }

    [Test]
    public async Task Commit_withTwoCoachesOnSameTeam_throwsDbUpdateException()
    {
        await using var context = NewContext();
        context.TeamMembers.Add(TeamMemberMother.DomainTeamMember(role: TeamMemberRole.Coach));
        context.TeamMembers.Add(
            TeamMemberMother.DomainTeamMember(userId: OtherUserId, role: TeamMemberRole.Coach));

        Assert.That(async () => await context.SaveChangesAsync(), Throws.InstanceOf<DbUpdateException>());
    }

    [Test]
    public async Task Commit_withTwoPlayersOnSameTeam_succeeds()
    {
        await using var context = NewContext();
        context.TeamMembers.Add(TeamMemberMother.DomainTeamMember(role: TeamMemberRole.Player));
        context.TeamMembers.Add(
            TeamMemberMother.DomainTeamMember(userId: OtherUserId, role: TeamMemberRole.Player));

        await context.SaveChangesAsync();

        Assert.That(await context.TeamMembers.CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task ExistsForUserAndTeamAsync_reflectsWhetherRowExists()
    {
        await using var seed = NewContext();
        seed.TeamMembers.Add(TeamMemberMother.DomainTeamMember());
        await seed.SaveChangesAsync();

        await using var context = NewContext();
        var repository = new EfTeamMemberRepository(context);

        Assert.That(
            await repository.ExistsForUserAndTeamAsync(TeamMemberMother.TeamId, TeamMemberMother.UserId), Is.True);
        Assert.That(
            await repository.ExistsForUserAndTeamAsync(TeamMemberMother.TeamId, OtherUserId), Is.False);
    }

    [Test]
    public async Task ExistsWithRoleAsync_reflectsWhetherARoleIsTaken()
    {
        await using var seed = NewContext();
        seed.TeamMembers.Add(TeamMemberMother.DomainTeamMember(role: TeamMemberRole.Delegate));
        await seed.SaveChangesAsync();

        await using var context = NewContext();
        var repository = new EfTeamMemberRepository(context);

        Assert.That(
            await repository.ExistsWithRoleAsync(TeamMemberMother.TeamId, TeamMemberRole.Delegate), Is.True);
        Assert.That(
            await repository.ExistsWithRoleAsync(TeamMemberMother.TeamId, TeamMemberRole.Coach), Is.False);
    }

    [Test]
    public async Task GetByTeamIdAsync_returnsOnlyThatTeamsMembers()
    {
        var otherTeamId = Guid.NewGuid();
        await using (var seed = NewContext())
        {
            seed.Teams.Add(TeamMother.DomainTeam(id: otherTeamId, name: "Rivals"));
            seed.TeamMembers.Add(TeamMemberMother.DomainTeamMember(role: TeamMemberRole.Player));
            seed.TeamMembers.Add(
                TeamMemberMother.DomainTeamMember(
                    userId: OtherUserId, teamId: otherTeamId, role: TeamMemberRole.Player));
            await seed.SaveChangesAsync();
        }

        await using var context = NewContext();
        var result = await new EfTeamMemberRepository(context).GetByTeamIdAsync(TeamMemberMother.TeamId);

        Assert.That(result.Select(m => m.UserId), Is.EquivalentTo(new[] { TeamMemberMother.UserId }));
    }

    [Test]
    public async Task GetByUserIdAsync_returnsOnlyThatUsersMemberships()
    {
        await using (var seed = NewContext())
        {
            seed.TeamMembers.Add(TeamMemberMother.DomainTeamMember(role: TeamMemberRole.Player));
            seed.TeamMembers.Add(
                TeamMemberMother.DomainTeamMember(userId: OtherUserId, role: TeamMemberRole.Coach));
            await seed.SaveChangesAsync();
        }

        await using var context = NewContext();
        var result = await new EfTeamMemberRepository(context).GetByUserIdAsync(TeamMemberMother.UserId);

        Assert.That(result.Select(m => m.UserId), Is.EquivalentTo(new[] { TeamMemberMother.UserId }));
    }

    [Test]
    public async Task GetByTeamAndUserForUpdateAsync_tracksTheEntity()
    {
        await using (var seed = NewContext())
        {
            seed.TeamMembers.Add(TeamMemberMother.DomainTeamMember());
            await seed.SaveChangesAsync();
        }

        await using var context = NewContext();
        await new EfTeamMemberRepository(context)
            .GetByTeamAndUserForUpdateAsync(TeamMemberMother.TeamId, TeamMemberMother.UserId);

        Assert.That(context.ChangeTracker.Entries<TeamMember>(), Is.Not.Empty);
    }

    [Test]
    public async Task Remove_thenCommit_deletesTheMember()
    {
        var member = TeamMemberMother.DomainTeamMember();
        await using (var seed = NewContext())
        {
            seed.TeamMembers.Add(member);
            await seed.SaveChangesAsync();
        }

        await using (var act = NewContext())
        {
            var repository = new EfTeamMemberRepository(act);
            var unitOfWork = new UnitOfWork(act);
            var tracked = await repository.GetByTeamAndUserForUpdateAsync(
                TeamMemberMother.TeamId, TeamMemberMother.UserId);
            repository.Remove(tracked!);
            await unitOfWork.SaveChangesAsync();
        }

        await using var assert = NewContext();
        Assert.That(await assert.TeamMembers.AnyAsync(m => m.Id == member.Id), Is.False);
    }
}
