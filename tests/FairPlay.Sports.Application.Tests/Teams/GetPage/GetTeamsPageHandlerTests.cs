using FairPlay.Sports.Application.Common.Querying;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Application.Teams.GetPage;
using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.TestSupport.Teams;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Teams.GetPage;

[TestFixture]
public class GetTeamsPageHandlerTests
{
    private ITeamRepository _repository = null!;
    private GetTeamsPageHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<ITeamRepository>();
        _handler = new GetTeamsPageHandler(_repository);
    }

    [Test]
    public async Task Handle_forwardsPagingAndFilterToRepository()
    {
        var filter = new TeamFilter(Name: "sev", Active: true);
        _repository.GetPageAsync(
                Arg.Any<IQueryFilter<Team>>(), Arg.Any<IQuerySort<Team>>(),
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Team>([], Page: 3, PageSize: 15, TotalCount: 0));

        await _handler.Handle(new GetTeamsPageQuery(3, 15, "-name", filter), CancellationToken.None);

        await _repository.Received(1).GetPageAsync(
            filter,
            Arg.Is<IQuerySort<Team>>(sort => sort is TeamSort),
            3,
            15,
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_mapsTheDomainPageToDtos_keepingMetadata()
    {
        var teams = new List<Team>
        {
            TeamMother.DomainTeam(name: "Alpha FC", city: "Sevilla"),
            TeamMother.DomainTeam(name: "Bravo FC", city: "Cadiz")
        };
        _repository.GetPageAsync(
                Arg.Any<IQueryFilter<Team>>(), Arg.Any<IQuerySort<Team>>(),
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Team>(teams, Page: 2, PageSize: 2, TotalCount: 7));

        var result = await _handler.Handle(
            new GetTeamsPageQuery(2, 2, null, new TeamFilter()), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.Items.Select(dto => dto.Name), Is.EqualTo(new[] { "Alpha FC", "Bravo FC" }));
            Assert.That(result.Value!.Items[0], Is.TypeOf<TeamDto>());
            Assert.That(result.Value!.Page, Is.EqualTo(2));
            Assert.That(result.Value!.PageSize, Is.EqualTo(2));
            Assert.That(result.Value!.TotalCount, Is.EqualTo(7));
            Assert.That(result.Value!.TotalPages, Is.EqualTo(4));
        });
    }

    [Test]
    public async Task Handle_forwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        _repository.GetPageAsync(
                Arg.Any<IQueryFilter<Team>>(), Arg.Any<IQuerySort<Team>>(),
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Team>([], Page: 1, PageSize: 20, TotalCount: 0));

        await _handler.Handle(new GetTeamsPageQuery(1, 20, null, new TeamFilter()), cts.Token);

        await _repository.Received(1).GetPageAsync(
            Arg.Any<IQueryFilter<Team>>(), Arg.Any<IQuerySort<Team>>(),
            1, 20, cts.Token);
    }
}
