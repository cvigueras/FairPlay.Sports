using FairPlay.Sports.Application.Common.Querying;
using FairPlay.Sports.Application.Standings.GetPage;
using FairPlay.Sports.Domain.Standings;
using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.Infrastructure.Persistence;
using FairPlay.Sports.Infrastructure.Standings;
using FairPlay.Sports.TestSupport.Standings;
using FairPlay.Sports.TestSupport.Teams;
using Microsoft.EntityFrameworkCore;

namespace FairPlay.Sports.Infrastructure.Tests.Standings;

[TestFixture]
public class EfStandingRepositoryTests : RepositoryTestBase
{
    private static async Task<Team> SeedTeamAsync()
    {
        var team = TeamMother.DomainTeam(name: $"Team {Guid.NewGuid():N}");
        await using var context = NewContext();
        context.Teams.Add(team);
        await context.SaveChangesAsync();
        return team;
    }

    private static async Task<IReadOnlyList<Team>> SeedTeamsAsync(int count)
    {
        var teams = Enumerable.Range(0, count).Select(_ => TeamMother.DomainTeam(name: $"Team {Guid.NewGuid():N}")).ToArray();
        await using var context = NewContext();
        context.Teams.AddRange(teams);
        await context.SaveChangesAsync();
        return teams;
    }

    private static async Task SeedAsync(params Standing[] standings)
    {
        await using var context = NewContext();
        context.Standings.AddRange(standings);
        await context.SaveChangesAsync();
    }

    [Test]
    public async Task AddAsync_thenCommit_persistsEveryColumn()
    {
        var team = await SeedTeamAsync();
        var standing = StandingMother.DomainStanding(
            teamId: team.Id, points: 10, played: 5, won: 3, drawn: 1, lost: 1, goalsFor: 12, goalsAgainst: 7);

        await using (var arrange = NewContext())
        {
            var repository = new EfStandingRepository(arrange);
            var unitOfWork = new UnitOfWork(arrange);
            await repository.AddAsync(standing);
            await unitOfWork.SaveChangesAsync();
        }

        await using var assert = NewContext();
        var persisted = await assert.Standings.AsNoTracking().SingleOrDefaultAsync(s => s.Id == standing.Id);
        Assert.That(persisted, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(persisted!.TeamId, Is.EqualTo(team.Id));
            Assert.That(persisted.Points, Is.EqualTo(10));
            Assert.That(persisted.Played, Is.EqualTo(5));
            Assert.That(persisted.Won, Is.EqualTo(3));
            Assert.That(persisted.Drawn, Is.EqualTo(1));
            Assert.That(persisted.Lost, Is.EqualTo(1));
            Assert.That(persisted.GoalsFor, Is.EqualTo(12));
            Assert.That(persisted.GoalsAgainst, Is.EqualTo(7));
            Assert.That(persisted.CreatedAt, Is.EqualTo(standing.CreatedAt).Within(TimeSpan.FromMilliseconds(10)));
        });
    }

    [Test]
    public async Task AddAsync_withoutCommit_doesNotPersist()
    {
        var team = await SeedTeamAsync();
        var standing = StandingMother.DomainStanding(teamId: team.Id);

        await using (var arrange = NewContext())
        {
            var repository = new EfStandingRepository(arrange);
            await repository.AddAsync(standing);
            // no unit-of-work commit
        }

        await using var assert = NewContext();
        Assert.That(await assert.Standings.AnyAsync(s => s.Id == standing.Id), Is.False);
    }

    [Test]
    public async Task Commit_withDuplicateTeamId_throwsDbUpdateException()
    {
        var team = await SeedTeamAsync();
        await SeedAsync(StandingMother.DomainStanding(teamId: team.Id));

        await using var context = NewContext();
        var repository = new EfStandingRepository(context);
        var unitOfWork = new UnitOfWork(context);
        await repository.AddAsync(StandingMother.DomainStanding(teamId: team.Id));

        Assert.That(async () => await unitOfWork.SaveChangesAsync(), Throws.InstanceOf<DbUpdateException>());
    }

    [Test]
    public async Task GetByIdForUpdateAsync_tracksTheEntity()
    {
        var team = await SeedTeamAsync();
        var standing = StandingMother.DomainStanding(teamId: team.Id);
        await SeedAsync(standing);

        await using var context = NewContext();
        await new EfStandingRepository(context).GetByIdForUpdateAsync(standing.Id);

        Assert.That(context.ChangeTracker.Entries<Standing>(), Is.Not.Empty);
    }

    [Test]
    public async Task GetByIdAsync_whenPresent_returnsTheStanding_untracked()
    {
        var team = await SeedTeamAsync();
        var standing = StandingMother.DomainStanding(teamId: team.Id);
        await SeedAsync(standing);

        await using var context = NewContext();
        var found = await new EfStandingRepository(context).GetByIdAsync(standing.Id);

        Assert.That(found, Is.Not.Null);
        Assert.That(found!.Id, Is.EqualTo(standing.Id));
        Assert.That(context.ChangeTracker.Entries<Standing>(), Is.Empty);
    }

    [Test]
    public async Task GetByIdAsync_whenMissing_returnsNull()
    {
        await using var context = NewContext();

        Assert.That(await new EfStandingRepository(context).GetByIdAsync(Guid.NewGuid()), Is.Null);
    }

    [Test]
    public async Task ExistsByTeamIdAsync_reflectsWhetherRowExists()
    {
        var team = await SeedTeamAsync();
        await SeedAsync(StandingMother.DomainStanding(teamId: team.Id));

        await using var context = NewContext();
        var repository = new EfStandingRepository(context);

        Assert.That(await repository.ExistsByTeamIdAsync(team.Id), Is.True);
        Assert.That(await repository.ExistsByTeamIdAsync(Guid.NewGuid()), Is.False);
    }

    [Test]
    public async Task RemoveAsync_thenCommit_deletesTheRow()
    {
        var team = await SeedTeamAsync();
        var standing = StandingMother.DomainStanding(teamId: team.Id);
        await SeedAsync(standing);

        await using (var act = NewContext())
        {
            var repository = new EfStandingRepository(act);
            var unitOfWork = new UnitOfWork(act);
            var tracked = await repository.GetByIdForUpdateAsync(standing.Id);
            await repository.RemoveAsync(tracked!);
            await unitOfWork.SaveChangesAsync();
        }

        await using var assert = NewContext();
        Assert.That(await assert.Standings.AnyAsync(s => s.Id == standing.Id), Is.False);
    }

    private static async Task<PagedResult<Standing>> GetPageAsync(
        int page, int pageSize, StandingFilter? filter = null, string? sort = null)
    {
        await using var context = NewContext();
        return await new EfStandingRepository(context).GetPageAsync(
            filter ?? new StandingFilter(), new StandingSort(sort), page, pageSize);
    }

    [Test]
    public async Task GetPageAsync_defaultSort_ordersByPointsDescending()
    {
        var teams = await SeedTeamsAsync(3);
        await SeedAsync(
            StandingMother.DomainStanding(teamId: teams[0].Id, points: 5, played: 0, won: 0, drawn: 0, lost: 0, goalsFor: 0, goalsAgainst: 0),
            StandingMother.DomainStanding(teamId: teams[1].Id, points: 15, played: 0, won: 0, drawn: 0, lost: 0, goalsFor: 0, goalsAgainst: 0),
            StandingMother.DomainStanding(teamId: teams[2].Id, points: 10, played: 0, won: 0, drawn: 0, lost: 0, goalsFor: 0, goalsAgainst: 0));

        var page = await GetPageAsync(page: 1, pageSize: 10);

        Assert.That(page.Items.Select(s => s.Points), Is.EqualTo(new[] { 15, 10, 5 }));
    }

    [Test]
    public async Task GetPageAsync_appliesSkipTake_andReportsFullTotalCount()
    {
        var teams = await SeedTeamsAsync(5);
        await SeedAsync(teams
            .Select((team, i) => StandingMother.DomainStanding(
                teamId: team.Id, points: i, played: 0, won: 0, drawn: 0, lost: 0, goalsFor: 0, goalsAgainst: 0))
            .ToArray());

        var page = await GetPageAsync(page: 2, pageSize: 2, sort: "points");

        Assert.Multiple(() =>
        {
            Assert.That(page.Items.Select(s => s.Points), Is.EqualTo(new[] { 2, 3 }));
            Assert.That(page.Page, Is.EqualTo(2));
            Assert.That(page.PageSize, Is.EqualTo(2));
            Assert.That(page.TotalCount, Is.EqualTo(5));
            Assert.That(page.TotalPages, Is.EqualTo(3));
            Assert.That(page.HasNext, Is.True);
            Assert.That(page.HasPrevious, Is.True);
        });
    }

    [Test]
    public async Task GetPageAsync_filtersByTeamId()
    {
        var teams = await SeedTeamsAsync(2);
        await SeedAsync(
            StandingMother.DomainStanding(teamId: teams[0].Id),
            StandingMother.DomainStanding(teamId: teams[1].Id));

        var page = await GetPageAsync(page: 1, pageSize: 10, filter: new StandingFilter(TeamId: teams[0].Id));

        Assert.That(page.Items.Single().TeamId, Is.EqualTo(teams[0].Id));
    }

    [Test]
    public async Task GetPageAsync_whenEmpty_returnsEmptyPageWithZeroTotal()
    {
        var page = await GetPageAsync(page: 1, pageSize: 10);

        Assert.Multiple(() =>
        {
            Assert.That(page.Items, Is.Empty);
            Assert.That(page.TotalCount, Is.EqualTo(0));
            Assert.That(page.TotalPages, Is.EqualTo(0));
            Assert.That(page.HasNext, Is.False);
            Assert.That(page.HasPrevious, Is.False);
        });
    }
}
