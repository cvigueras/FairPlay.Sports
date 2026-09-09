using FairPlay.Sports.Application.Common.Querying;
using FairPlay.Sports.Application.Teams.GetPage;
using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.Infrastructure.Persistence;
using FairPlay.Sports.Infrastructure.Teams;
using FairPlay.Sports.TestSupport.Teams;
using Microsoft.EntityFrameworkCore;

namespace FairPlay.Sports.Infrastructure.Tests.Teams;

[TestFixture]
public class EfTeamRepositoryTests : RepositoryTestBase
{
    private static async Task SeedAsync(params Team[] teams)
    {
        await using var context = NewContext();
        context.Teams.AddRange(teams);
        await context.SaveChangesAsync();
    }

    [Test]
    public async Task AddAsync_thenCommit_persistsEveryColumn()
    {
        var team = TeamMother.DomainTeam(
            city: "Sevilla", type: FootballType.BeachSoccer,
            division: Division.HonorDivision, category: AgeCategory.Under12);

        await using (var arrange = NewContext())
        {
            var repository = new EfTeamRepository(arrange);
            var unitOfWork = new UnitOfWork(arrange);
            await repository.AddAsync(team);
            await unitOfWork.SaveChangesAsync();
        }

        await using var assert = NewContext();
        var persisted = await assert.Teams.AsNoTracking().SingleOrDefaultAsync(t => t.Id == team.Id);
        Assert.That(persisted, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(persisted!.Name, Is.EqualTo(team.Name));
            Assert.That(persisted.Coach, Is.EqualTo(team.Coach));
            Assert.That(persisted.City, Is.EqualTo("Sevilla"));
            Assert.That(persisted.Classification.Type, Is.EqualTo(FootballType.BeachSoccer));
            Assert.That(persisted.Classification.Division, Is.EqualTo(Division.HonorDivision));
            Assert.That(persisted.Classification.Category, Is.EqualTo(AgeCategory.Under12));
            Assert.That(persisted.HasCrest, Is.False);
            Assert.That(persisted.Active, Is.True);
            Assert.That(persisted.CreatedAt, Is.EqualTo(team.CreatedAt).Within(TimeSpan.FromMilliseconds(10)));
        });
    }

    [Test]
    public async Task EnumColumns_areStoredAsStrings()
    {
        var team = TeamMother.DomainTeam(type: FootballType.Futsal, division: Division.RegionalLeague, category: AgeCategory.Under10);
        await SeedAsync(team);

        await using var context = NewContext();
        var stored = await context.Database
            .SqlQuery<string>($"""SELECT CONCAT("Type", '|', "Division", '|', "Category") AS "Value" FROM "Teams" WHERE "Id" = {team.Id}""")
            .SingleAsync();

        Assert.That(stored, Is.EqualTo("Futsal|RegionalLeague|Under10"));
    }

    [Test]
    public async Task AddAsync_withoutCommit_doesNotPersist()
    {
        var team = TeamMother.DomainTeam();

        await using (var arrange = NewContext())
        {
            var repository = new EfTeamRepository(arrange);
            await repository.AddAsync(team);
            // no unit-of-work commit
        }

        await using var assert = NewContext();
        Assert.That(await assert.Teams.AnyAsync(t => t.Id == team.Id), Is.False);
    }

    [Test]
    public async Task Commit_withDuplicateName_throwsDbUpdateException()
    {
        await SeedAsync(TeamMother.DomainTeam(name: "Dupe FC"));

        await using var context = NewContext();
        var repository = new EfTeamRepository(context);
        var unitOfWork = new UnitOfWork(context);
        await repository.AddAsync(TeamMother.DomainTeam(name: "Dupe FC"));

        Assert.That(async () => await unitOfWork.SaveChangesAsync(), Throws.InstanceOf<DbUpdateException>());
    }

    [Test]
    public async Task Commit_withNameOverMaxLength_throwsDbUpdateException()
    {
        await using var context = NewContext();
        context.Teams.Add(TeamMother.DomainTeam(name: new string('x', 101)));

        Assert.That(async () => await context.SaveChangesAsync(), Throws.InstanceOf<DbUpdateException>());
    }

    [Test]
    public async Task SetCrest_thenCommit_roundTripsTheImageBytes()
    {
        var team = TeamMother.DomainTeam();
        await SeedAsync(team);

        await using (var act = NewContext())
        {
            var repository = new EfTeamRepository(act);
            var unitOfWork = new UnitOfWork(act);
            var tracked = await repository.GetByIdForUpdateAsync(team.Id);
            tracked!.SetCrest(TeamMother.CrestBytes, TeamMother.CrestContentType);
            await unitOfWork.SaveChangesAsync();
        }

        await using var assert = NewContext();
        var crest = await new EfTeamRepository(assert).GetCrestAsync(team.Id);
        Assert.That(crest, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(crest!.Content, Is.EqualTo(TeamMother.CrestBytes));
            Assert.That(crest.ContentType, Is.EqualTo(TeamMother.CrestContentType));
        });
    }

    [Test]
    public async Task GetCrestAsync_whenTeamHasNoCrest_returnsNull()
    {
        var team = TeamMother.DomainTeam();
        await SeedAsync(team);

        await using var context = NewContext();
        Assert.That(await new EfTeamRepository(context).GetCrestAsync(team.Id), Is.Null);
    }

    [Test]
    public async Task GetByIdForUpdateAsync_tracksTheEntity()
    {
        var team = TeamMother.DomainTeam();
        await SeedAsync(team);

        await using var context = NewContext();
        await new EfTeamRepository(context).GetByIdForUpdateAsync(team.Id);

        Assert.That(context.ChangeTracker.Entries<Team>(), Is.Not.Empty);
    }

    [Test]
    public async Task GetByIdAsync_whenPresent_returnsTheTeam_untracked()
    {
        var team = TeamMother.DomainTeam();
        await SeedAsync(team);

        await using var context = NewContext();
        var found = await new EfTeamRepository(context).GetByIdAsync(team.Id);

        Assert.That(found, Is.Not.Null);
        Assert.That(found!.Id, Is.EqualTo(team.Id));
        Assert.That(context.ChangeTracker.Entries<Team>(), Is.Empty);
    }

    [Test]
    public async Task GetByIdAsync_whenMissing_returnsNull()
    {
        await using var context = NewContext();

        Assert.That(await new EfTeamRepository(context).GetByIdAsync(Guid.NewGuid()), Is.Null);
    }

    private static async Task<PagedResult<Team>> GetPageAsync(
        int page, int pageSize, TeamFilter? filter = null, string? sort = null)
    {
        await using var context = NewContext();
        return await new EfTeamRepository(context).GetPageAsync(
            filter ?? new TeamFilter(), new TeamSort(sort), page, pageSize);
    }

    [Test]
    public async Task GetPageAsync_defaultSort_ordersByNameAscending()
    {
        await SeedAsync(
            TeamMother.DomainTeam(name: "Charlie FC"),
            TeamMother.DomainTeam(name: "Alpha FC"),
            TeamMother.DomainTeam(name: "Bravo FC"));

        var page = await GetPageAsync(page: 1, pageSize: 10);

        Assert.That(page.Items.Select(t => t.Name), Is.EqualTo(new[] { "Alpha FC", "Bravo FC", "Charlie FC" }));
    }

    [Test]
    public async Task GetPageAsync_descendingSort_reversesOrder()
    {
        await SeedAsync(
            TeamMother.DomainTeam(name: "Alpha FC"),
            TeamMother.DomainTeam(name: "Bravo FC"),
            TeamMother.DomainTeam(name: "Charlie FC"));

        var page = await GetPageAsync(page: 1, pageSize: 10, sort: "-name");

        Assert.That(page.Items.Select(t => t.Name), Is.EqualTo(new[] { "Charlie FC", "Bravo FC", "Alpha FC" }));
    }

    [Test]
    public async Task GetPageAsync_appliesSkipTake_andReportsFullTotalCount()
    {
        await SeedAsync(
            TeamMother.DomainTeam(name: "Team A"),
            TeamMother.DomainTeam(name: "Team B"),
            TeamMother.DomainTeam(name: "Team C"),
            TeamMother.DomainTeam(name: "Team D"),
            TeamMother.DomainTeam(name: "Team E"));

        var page = await GetPageAsync(page: 2, pageSize: 2);

        Assert.Multiple(() =>
        {
            Assert.That(page.Items.Select(t => t.Name), Is.EqualTo(new[] { "Team C", "Team D" }));
            Assert.That(page.Page, Is.EqualTo(2));
            Assert.That(page.PageSize, Is.EqualTo(2));
            Assert.That(page.TotalCount, Is.EqualTo(5));
            Assert.That(page.TotalPages, Is.EqualTo(3));
            Assert.That(page.HasNext, Is.True);
            Assert.That(page.HasPrevious, Is.True);
        });
    }

    [Test]
    public async Task GetPageAsync_filtersByNameContains_caseInsensitively()
    {
        await SeedAsync(
            TeamMother.DomainTeam(name: "Real Betis"),
            TeamMother.DomainTeam(name: "Sevilla FC"));

        var page = await GetPageAsync(page: 1, pageSize: 10, filter: new TeamFilter(Name: "betis"));

        Assert.That(page.TotalCount, Is.EqualTo(1));
        Assert.That(page.Items.Single().Name, Is.EqualTo("Real Betis"));
    }

    [Test]
    public async Task GetPageAsync_filtersByCoachContains_caseInsensitively()
    {
        await SeedAsync(
            TeamMother.DomainTeam(name: "Real Betis", coach: "Manuel Pellegrini"),
            TeamMother.DomainTeam(name: "Sevilla FC", coach: "Garcia Pimienta"));

        var page = await GetPageAsync(page: 1, pageSize: 10, filter: new TeamFilter(Coach: "pELLEGRINI"));

        Assert.That(page.TotalCount, Is.EqualTo(1));
        Assert.That(page.Items.Single().Name, Is.EqualTo("Real Betis"));
    }

    [Test]
    public async Task GetPageAsync_textFilters_ignoreAccents()
    {
        await SeedAsync(
            TeamMother.DomainTeam(name: "Adra CF", coach: "Diego Martínez", city: "Almería"),
            TeamMother.DomainTeam(name: "Sevilla FC", coach: "Garcia Pimienta", city: "Sevilla"));

        var page = await GetPageAsync(
            page: 1, pageSize: 10,
            filter: new TeamFilter(Coach: "martinez", City: "almeria"));

        Assert.That(page.Items.Select(t => t.Name), Is.EqualTo(new[] { "Adra CF" }));
    }

    [Test]
    public async Task GetPageAsync_filtersByActiveAndType()
    {
        await SeedAsync(
            TeamMother.DomainTeam(name: "Active Futsal", type: FootballType.Futsal, active: true),
            TeamMother.DomainTeam(name: "Inactive Futsal", type: FootballType.Futsal, active: false),
            TeamMother.DomainTeam(name: "Active Football11", type: FootballType.Football11, active: true));

        var page = await GetPageAsync(
            page: 1, pageSize: 10,
            filter: new TeamFilter(Type: FootballType.Futsal, Active: true));

        Assert.That(page.Items.Select(t => t.Name), Is.EqualTo(new[] { "Active Futsal" }));
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

    [Test]
    public async Task ExistsByNameAsync_reflectsWhetherRowExists()
    {
        await SeedAsync(TeamMother.DomainTeam(name: "Known FC"));

        await using var context = NewContext();
        var repository = new EfTeamRepository(context);

        Assert.That(await repository.ExistsByNameAsync("Known FC"), Is.True);
        Assert.That(await repository.ExistsByNameAsync("Unknown FC"), Is.False);
    }
}
