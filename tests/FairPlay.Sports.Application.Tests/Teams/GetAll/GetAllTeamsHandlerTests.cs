using FairPlay.Sports.TestSupport.Teams;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Application.Teams.GetAll;
using FairPlay.Sports.Domain.Teams;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Teams.GetAll;

/// <summary>
/// Unit tests for <see cref="GetAllTeamsHandler"/>: the repository port is mocked, the real
/// <c>TeamDto.FromDomain</c> mapping runs. Scope is the handler's own logic - fetch, map,
/// wrap in a successful Result. Test data comes from <see cref="TeamMother"/>.
/// </summary>
[TestFixture]
public class GetAllTeamsHandlerTests
{
    private ITeamRepository _repository = null!;
    private GetAllTeamsHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<ITeamRepository>();
        _handler = new GetAllTeamsHandler(_repository);
    }

    [Test]
    public async Task Handle_MapsEveryTeamToDto()
    {
        var first = TeamMother.DomainTeam(
            name: "FairPlay FC", coach: "Marta Rios", city: "Sevilla",
            type: FootballType.Futsal, division: Division.First, category: AgeCategory.Under19);
        var second = TeamMother.DomainTeam(
            name: "Rivals CF", coach: "Leo Pena", city: "Cadiz",
            type: FootballType.Football11, division: Division.HonorDivision, category: AgeCategory.Under16);
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Team> { first, second });

        var result = await _handler.Handle(new GetAllTeamsQuery(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(result.Value![0].Id, Is.EqualTo(first.Id));
            Assert.That(result.Value![0].Name, Is.EqualTo("FairPlay FC"));
            Assert.That(result.Value![0].City, Is.EqualTo("Sevilla"));
            Assert.That(result.Value![0].Type, Is.EqualTo(FootballType.Futsal));
            Assert.That(result.Value![0].Division, Is.EqualTo(Division.First));
            Assert.That(result.Value![0].Category, Is.EqualTo(AgeCategory.Under19));
            Assert.That(result.Value![0].HasCrest, Is.False);
            Assert.That(result.Value![0].Active, Is.True);
            Assert.That(result.Value![1].Name, Is.EqualTo("Rivals CF"));
            Assert.That(result.Value![1].Type, Is.EqualTo(FootballType.Football11));
        });
    }

    [Test]
    public async Task Handle_WhenRepositoryEmpty_ReturnsSuccessWithEmptyList()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Team>());

        var result = await _handler.Handle(new GetAllTeamsQuery(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Empty);
    }

    [Test]
    public async Task Handle_ForwardsCancellationTokenToRepository()
    {
        using var cts = new CancellationTokenSource();
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Team>());

        await _handler.Handle(new GetAllTeamsQuery(), cts.Token);

        await _repository.Received(1).GetAllAsync(cts.Token);
    }
}
