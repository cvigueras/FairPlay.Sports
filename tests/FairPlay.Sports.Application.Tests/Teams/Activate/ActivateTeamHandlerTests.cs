using FairPlay.Sports.Application.Common;
using FairPlay.Sports.TestSupport.Teams;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Application.Teams.Activate;
using FairPlay.Sports.Domain.Teams;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Teams.Activate;

[TestFixture]
public class ActivateTeamHandlerTests
{
    private ITeamRepository _repository = null!;
    private ActivateTeamHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<ITeamRepository>();
        _handler = new ActivateTeamHandler(_repository);
    }

    [Test]
    public async Task Handle_WhenTeamExists_ActivatesTheTrackedAggregate_AndReturnsSuccess()
    {
        var team = TeamMother.DomainTeam(active: false);
        _repository.GetByIdForUpdateAsync(team.Id, Arg.Any<CancellationToken>()).Returns(team);

        var result = await _handler.Handle(new ActivateTeamCommand(team.Id), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(team.Active, Is.True);
    }

    [Test]
    public async Task Handle_WhenTeamMissing_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdForUpdateAsync(id, Arg.Any<CancellationToken>()).Returns((Team?)null);

        var result = await _handler.Handle(new ActivateTeamCommand(id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
            Assert.That(result.Error, Does.Contain(id.ToString()));
        });
    }

    [Test]
    public async Task Handle_LoadsTheTeamThroughTheTrackedGetter()
    {
        var team = TeamMother.DomainTeam(active: false);
        _repository.GetByIdForUpdateAsync(team.Id, Arg.Any<CancellationToken>()).Returns(team);

        await _handler.Handle(new ActivateTeamCommand(team.Id), CancellationToken.None);

        await _repository.Received(1).GetByIdForUpdateAsync(team.Id, Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
