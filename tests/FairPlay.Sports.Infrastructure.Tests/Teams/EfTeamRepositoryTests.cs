using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.Infrastructure.Persistence;
using FairPlay.Sports.Infrastructure.Teams;
using FairPlay.Sports.TestSupport.Teams;
using Microsoft.EntityFrameworkCore;

namespace FairPlay.Sports.Infrastructure.Tests.Teams;

/// <summary>
/// Integration tests for <see cref="EfTeamRepository"/> against a real SQL Server (Testcontainers).
/// They cover what an in-memory provider cannot: the unique index on Name, the column length
/// limits, the enum-to-string conversions, the crest <c>varbinary</c> round-trip, tracked vs
/// no-tracking reads, and that writes only land once the unit of work commits. Test data comes
/// from <see cref="TeamMother"/>.
/// </summary>
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
            Assert.That(persisted.Type, Is.EqualTo(FootballType.BeachSoccer));
            Assert.That(persisted.Division, Is.EqualTo(Division.HonorDivision));
            Assert.That(persisted.Category, Is.EqualTo(AgeCategory.Under12));
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
            .SqlQuery<string>($"SELECT CONCAT([Type], '|', [Division], '|', [Category]) AS Value FROM Teams WHERE Id = {team.Id}")
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

    [Test]
    public async Task GetAllAsync_ordersByNameAscending()
    {
        await SeedAsync(
            TeamMother.DomainTeam(name: "Charlie FC"),
            TeamMother.DomainTeam(name: "Alpha FC"),
            TeamMother.DomainTeam(name: "Bravo FC"));

        await using var context = NewContext();
        var all = await new EfTeamRepository(context).GetAllAsync();

        Assert.That(all.Select(t => t.Name), Is.EqualTo(new[] { "Alpha FC", "Bravo FC", "Charlie FC" }));
    }

    [Test]
    public async Task GetAllAsync_whenEmpty_returnsEmptyList()
    {
        await using var context = NewContext();

        Assert.That(await new EfTeamRepository(context).GetAllAsync(), Is.Empty);
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
