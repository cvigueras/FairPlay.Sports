using FairPlay.Sports.Application.Common.Querying;
using FairPlay.Sports.Application.Standings;
using FairPlay.Sports.Application.Standings.GetPage;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Domain.Standings;
using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.TestSupport.Standings;
using FairPlay.Sports.TestSupport.Teams;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Standings.GetPage;

[TestFixture]
public class GetStandingsPageHandlerTests
{
    private IStandingRepository _standings = null!;
    private ITeamRepository _teams = null!;
    private GetStandingsPageHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _standings = Substitute.For<IStandingRepository>();
        _teams = Substitute.For<ITeamRepository>();
        _handler = new GetStandingsPageHandler(_standings, _teams);
    }

    [Test]
    public async Task Handle_forwardsPagingAndFilterToRepository()
    {
        var filter = new StandingFilter(TeamId: Guid.NewGuid());
        _standings.GetPageAsync(
                Arg.Any<IQueryFilter<Standing>>(), Arg.Any<IQuerySort<Standing>>(),
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Standing>([], Page: 3, PageSize: 15, TotalCount: 0));

        await _handler.Handle(new GetStandingsPageQuery(3, 15, "-points", filter), CancellationToken.None);

        await _standings.Received(1).GetPageAsync(
            filter,
            Arg.Is<IQuerySort<Standing>>(sort => sort is StandingSort),
            3,
            15,
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_resolvesAClassificationFilter_toTeamIds_beforePaging()
    {
        var matchingTeamIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        _teams.GetIdsByClassificationAsync(
                FootballType.Futsal, Division.First, AgeCategory.Juveniles, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Guid>)matchingTeamIds);
        _standings.GetPageAsync(
                Arg.Any<IQueryFilter<Standing>>(), Arg.Any<IQuerySort<Standing>>(),
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Standing>([], Page: 1, PageSize: 20, TotalCount: 0));

        var filter = new StandingFilter(Type: FootballType.Futsal, Division: Division.First, Category: AgeCategory.Juveniles);
        await _handler.Handle(new GetStandingsPageQuery(1, 20, null, filter), CancellationToken.None);

        await _standings.Received(1).GetPageAsync(
            Arg.Is<IQueryFilter<Standing>>(f => ((StandingFilter)f).TeamIds!.SequenceEqual(matchingTeamIds)),
            Arg.Any<IQuerySort<Standing>>(),
            1,
            20,
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_mapsTheDomainPageToDtos_resolvingTeamNames_andKeepingMetadata()
    {
        var teamA = TeamMother.DomainTeam(name: "Alpha FC");
        var teamB = TeamMother.DomainTeam(name: "Bravo FC");
        var items = new List<Standing>
        {
            StandingMother.DomainStanding(teamId: teamA.Id),
            StandingMother.DomainStanding(teamId: teamB.Id)
        };
        _standings.GetPageAsync(
                Arg.Any<IQueryFilter<Standing>>(), Arg.Any<IQuerySort<Standing>>(),
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Standing>(items, Page: 2, PageSize: 2, TotalCount: 7));
        _teams.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Team>)[teamA, teamB]);

        var result = await _handler.Handle(
            new GetStandingsPageQuery(2, 2, null, new StandingFilter()), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.Items.Select(dto => dto.TeamName), Is.EqualTo(new[] { "Alpha FC", "Bravo FC" }));
            Assert.That(result.Value!.Page, Is.EqualTo(2));
            Assert.That(result.Value!.PageSize, Is.EqualTo(2));
            Assert.That(result.Value!.TotalCount, Is.EqualTo(7));
            Assert.That(result.Value!.TotalPages, Is.EqualTo(4));
        });
    }
}
