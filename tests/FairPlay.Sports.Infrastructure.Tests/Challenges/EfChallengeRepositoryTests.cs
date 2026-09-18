using FairPlay.Sports.Domain.Challenges;
using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.Infrastructure.Challenges;
using FairPlay.Sports.Infrastructure.Persistence;
using FairPlay.Sports.TestSupport.Challenges;
using FairPlay.Sports.TestSupport.Teams;
using Microsoft.EntityFrameworkCore;

namespace FairPlay.Sports.Infrastructure.Tests.Challenges;

[TestFixture]
public class EfChallengeRepositoryTests : RepositoryTestBase
{
    private static async Task<(Team Challenger, Team Challenged)> SeedTeamsAsync()
    {
        var challenger = TeamMother.DomainTeam(name: $"Challenger {Guid.NewGuid():N}");
        var challenged = TeamMother.DomainTeam(name: $"Challenged {Guid.NewGuid():N}");
        await using var context = NewContext();
        context.Teams.AddRange(challenger, challenged);
        await context.SaveChangesAsync();
        return (challenger, challenged);
    }

    private static async Task SeedAsync(params Challenge[] challenges)
    {
        await using var context = NewContext();
        context.Challenges.AddRange(challenges);
        await context.SaveChangesAsync();
    }

    [Test]
    public async Task AddAsync_thenCommit_persistsEveryColumn()
    {
        var (challenger, challenged) = await SeedTeamsAsync();
        var challenge = ChallengeMother.DomainChallenge(challengerTeamId: challenger.Id, challengedTeamId: challenged.Id);

        await using (var arrange = NewContext())
        {
            var repository = new EfChallengeRepository(arrange);
            var unitOfWork = new UnitOfWork(arrange);
            await repository.AddAsync(challenge);
            await unitOfWork.SaveChangesAsync();
        }

        await using var assert = NewContext();
        var persisted = await assert.Challenges.AsNoTracking().SingleOrDefaultAsync(c => c.Id == challenge.Id);
        Assert.That(persisted, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(persisted!.ChallengerTeamId, Is.EqualTo(challenger.Id));
            Assert.That(persisted.ChallengedTeamId, Is.EqualTo(challenged.Id));
            Assert.That(persisted.Message, Is.EqualTo(ChallengeMother.Message));
            Assert.That(persisted.Status, Is.EqualTo(ChallengeStatus.Pending));
            Assert.That(persisted.RespondedAt, Is.Null);
            Assert.That(persisted.CreatedAt, Is.EqualTo(challenge.CreatedAt).Within(TimeSpan.FromMilliseconds(10)));
        });
    }

    [Test]
    public async Task AddAsync_withoutCommit_doesNotPersist()
    {
        var (challenger, challenged) = await SeedTeamsAsync();
        var challenge = ChallengeMother.DomainChallenge(challengerTeamId: challenger.Id, challengedTeamId: challenged.Id);

        await using (var arrange = NewContext())
        {
            var repository = new EfChallengeRepository(arrange);
            await repository.AddAsync(challenge);
            // no unit-of-work commit
        }

        await using var assert = NewContext();
        Assert.That(await assert.Challenges.AnyAsync(c => c.Id == challenge.Id), Is.False);
    }

    [Test]
    public async Task GetByIdForUpdateAsync_tracksTheEntity()
    {
        var (challenger, challenged) = await SeedTeamsAsync();
        var challenge = ChallengeMother.DomainChallenge(challengerTeamId: challenger.Id, challengedTeamId: challenged.Id);
        await SeedAsync(challenge);

        await using var context = NewContext();
        await new EfChallengeRepository(context).GetByIdForUpdateAsync(challenge.Id);

        Assert.That(context.ChangeTracker.Entries<Challenge>(), Is.Not.Empty);
    }

    [Test]
    public async Task GetByIdAsync_whenPresent_returnsTheChallenge_untracked()
    {
        var (challenger, challenged) = await SeedTeamsAsync();
        var challenge = ChallengeMother.DomainChallenge(challengerTeamId: challenger.Id, challengedTeamId: challenged.Id);
        await SeedAsync(challenge);

        await using var context = NewContext();
        var found = await new EfChallengeRepository(context).GetByIdAsync(challenge.Id);

        Assert.That(found, Is.Not.Null);
        Assert.That(found!.Id, Is.EqualTo(challenge.Id));
        Assert.That(context.ChangeTracker.Entries<Challenge>(), Is.Empty);
    }

    [Test]
    public async Task GetByIdAsync_whenMissing_returnsNull()
    {
        await using var context = NewContext();

        Assert.That(await new EfChallengeRepository(context).GetByIdAsync(Guid.NewGuid()), Is.Null);
    }

    [Test]
    public async Task GetByTeamIdAsync_returnsChallengesSentAndReceivedByTheTeam_ButNotOthers()
    {
        var (teamA, teamB) = await SeedTeamsAsync();
        var (_, teamC) = await SeedTeamsAsync();
        var sent = ChallengeMother.DomainChallenge(challengerTeamId: teamA.Id, challengedTeamId: teamB.Id);
        var received = ChallengeMother.DomainChallenge(challengerTeamId: teamC.Id, challengedTeamId: teamA.Id);
        var unrelated = ChallengeMother.DomainChallenge(challengerTeamId: teamB.Id, challengedTeamId: teamC.Id);
        await SeedAsync(sent, received, unrelated);

        await using var context = NewContext();
        var found = await new EfChallengeRepository(context).GetByTeamIdAsync(teamA.Id);

        Assert.That(found.Select(c => c.Id), Is.EquivalentTo(new[] { sent.Id, received.Id }));
    }
}
