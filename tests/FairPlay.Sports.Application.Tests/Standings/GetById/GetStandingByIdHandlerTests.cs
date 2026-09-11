using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Standings;
using FairPlay.Sports.Application.Standings.GetById;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Domain.Standings;
using FairPlay.Sports.TestSupport.Standings;
using FairPlay.Sports.TestSupport.Teams;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Standings.GetById;

[TestFixture]
public class GetStandingByIdHandlerTests
{
    private IStandingRepository _standings = null!;
    private ITeamRepository _teams = null!;
    private GetStandingByIdHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _standings = Substitute.For<IStandingRepository>();
        _teams = Substitute.For<ITeamRepository>();
        _handler = new GetStandingByIdHandler(_standings, _teams);
    }

    [Test]
    public async Task Handle_WhenMissing_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _standings.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Standing?)null);

        var result = await _handler.Handle(new GetStandingByIdQuery(id), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
    }

    [Test]
    public async Task Handle_WhenPresent_ReturnsDtoWithTeamName()
    {
        var team = TeamMother.DomainTeam();
        var standing = StandingMother.DomainStanding(teamId: team.Id);
        _standings.GetByIdAsync(standing.Id, Arg.Any<CancellationToken>()).Returns(standing);
        _teams.GetByIdAsync(team.Id, Arg.Any<CancellationToken>()).Returns(team);

        var result = await _handler.Handle(new GetStandingByIdQuery(standing.Id), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.Id, Is.EqualTo(standing.Id));
            Assert.That(result.Value!.TeamId, Is.EqualTo(team.Id));
            Assert.That(result.Value!.TeamName, Is.EqualTo(team.Name));
        });
    }
}
