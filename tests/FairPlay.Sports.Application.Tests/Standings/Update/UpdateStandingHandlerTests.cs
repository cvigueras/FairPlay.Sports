using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Standings;
using FairPlay.Sports.Application.Standings.Update;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Domain.Standings;
using FairPlay.Sports.TestSupport.Standings;
using FairPlay.Sports.TestSupport.Teams;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Standings.Update;

[TestFixture]
public class UpdateStandingHandlerTests
{
    private IStandingRepository _standings = null!;
    private ITeamRepository _teams = null!;
    private UpdateStandingHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _standings = Substitute.For<IStandingRepository>();
        _teams = Substitute.For<ITeamRepository>();
        _handler = new UpdateStandingHandler(_standings, _teams);
    }

    [Test]
    public async Task Handle_WhenStandingMissing_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _standings.GetByIdForUpdateAsync(id, Arg.Any<CancellationToken>()).Returns((Standing?)null);

        var result = await _handler.Handle(StandingMother.UpdateCommand(id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
        });
    }

    [Test]
    public async Task Handle_WhenValid_UpdatesStats_AndReturnsDto()
    {
        var team = TeamMother.DomainTeam();
        var standing = StandingMother.DomainStanding(teamId: team.Id, points: 0, played: 0, won: 0, drawn: 0, lost: 0, goalsFor: 0, goalsAgainst: 0);
        _standings.GetByIdForUpdateAsync(standing.Id, Arg.Any<CancellationToken>()).Returns(standing);
        _teams.GetByIdAsync(team.Id, Arg.Any<CancellationToken>()).Returns(team);

        var command = StandingMother.UpdateCommand(standing.Id);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.Id, Is.EqualTo(standing.Id));
            Assert.That(result.Value!.TeamName, Is.EqualTo(team.Name));
            Assert.That(result.Value!.Points, Is.EqualTo(StandingMother.Points));
            Assert.That(standing.Points, Is.EqualTo(StandingMother.Points));
            Assert.That(standing.Played, Is.EqualTo(StandingMother.Played));
        });
    }
}
