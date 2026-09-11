using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Standings;
using FairPlay.Sports.Application.Standings.Create;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Domain.Standings;
using FairPlay.Sports.TestSupport.Standings;
using FairPlay.Sports.TestSupport.Teams;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Standings.Create;

[TestFixture]
public class CreateStandingHandlerTests
{
    private static readonly DateTime Now = new(2026, 9, 2, 12, 0, 0, DateTimeKind.Utc);

    private IStandingRepository _standings = null!;
    private ITeamRepository _teams = null!;
    private IClock _clock = null!;
    private CreateStandingHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _standings = Substitute.For<IStandingRepository>();
        _teams = Substitute.For<ITeamRepository>();
        _clock = Substitute.For<IClock>();
        _clock.UtcNow.Returns(Now);
        _handler = new CreateStandingHandler(_standings, _teams, _clock);
    }

    [Test]
    public async Task Handle_WhenTeamMissing_ReturnsNotFound_AndDoesNotPersist()
    {
        var teamId = Guid.NewGuid();
        _teams.GetByIdAsync(teamId, Arg.Any<CancellationToken>()).Returns((Domain.Teams.Team?)null);

        var result = await _handler.Handle(StandingMother.Command(teamId), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
        });
        await _standings.DidNotReceive().AddAsync(Arg.Any<Standing>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenTeamAlreadyHasStanding_ReturnsFailure_AndDoesNotPersist()
    {
        var team = TeamMother.DomainTeam();
        _teams.GetByIdAsync(team.Id, Arg.Any<CancellationToken>()).Returns(team);
        _standings.ExistsByTeamIdAsync(team.Id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(StandingMother.Command(team.Id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.Validation));
            Assert.That(result.Error, Is.EqualTo(StandingMother.TeamAlreadyHasStanding(team.Id)));
        });
        await _standings.DidNotReceive().AddAsync(Arg.Any<Standing>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenValid_PersistsStanding_AndReturnsDto()
    {
        var team = TeamMother.DomainTeam();
        _teams.GetByIdAsync(team.Id, Arg.Any<CancellationToken>()).Returns(team);
        _standings.ExistsByTeamIdAsync(team.Id, Arg.Any<CancellationToken>()).Returns(false);

        var command = StandingMother.Command(team.Id);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.TeamId, Is.EqualTo(team.Id));
            Assert.That(result.Value!.TeamName, Is.EqualTo(team.Name));
            Assert.That(result.Value!.Points, Is.EqualTo(StandingMother.Points));
            Assert.That(result.Value!.Played, Is.EqualTo(StandingMother.Played));
            Assert.That(result.Value!.Won, Is.EqualTo(StandingMother.Won));
            Assert.That(result.Value!.Drawn, Is.EqualTo(StandingMother.Drawn));
            Assert.That(result.Value!.Lost, Is.EqualTo(StandingMother.Lost));
            Assert.That(result.Value!.GoalsFor, Is.EqualTo(StandingMother.GoalsFor));
            Assert.That(result.Value!.GoalsAgainst, Is.EqualTo(StandingMother.GoalsAgainst));
            Assert.That(result.Value!.Id, Is.Not.EqualTo(Guid.Empty));
        });

        await _standings.Received(1).AddAsync(
            Arg.Is<Standing>(standing =>
                standing.TeamId == team.Id &&
                standing.Points == StandingMother.Points &&
                standing.CreatedAt == Now),
            Arg.Any<CancellationToken>());
    }
}
