using FairPlay.Sports.Application.Common;
using FairPlay.Sports.TestSupport.Teams;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Application.Teams.UploadCrest;
using FairPlay.Sports.Domain.Teams;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Teams.UploadCrest;

/// <summary>
/// Unit tests for <see cref="UploadTeamCrestHandler"/>: the repository port is mocked, the real
/// <see cref="Team"/> aggregate is mutated. Scope is "load the tracked team, set the crest,
/// translate missing into NotFound". Structural checks on the image are a validator concern and
/// out of scope here. Test data comes from <see cref="TeamMother"/>.
/// </summary>
[TestFixture]
public class UploadTeamCrestHandlerTests
{
    private ITeamRepository _repository = null!;
    private UploadTeamCrestHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<ITeamRepository>();
        _handler = new UploadTeamCrestHandler(_repository);
    }

    [Test]
    public async Task Handle_WhenTeamExists_SetsCrestOnTheTrackedAggregate_AndReturnsSuccess()
    {
        var team = TeamMother.DomainTeam();
        _repository.GetByIdForUpdateAsync(team.Id, Arg.Any<CancellationToken>()).Returns(team);

        var command = new UploadTeamCrestCommand(team.Id, TeamMother.CrestBytes, TeamMother.CrestContentType);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(team.HasCrest, Is.True);
            Assert.That(team.Crest, Is.EqualTo(TeamMother.CrestBytes));
            Assert.That(team.CrestContentType, Is.EqualTo(TeamMother.CrestContentType));
        });
    }

    [Test]
    public async Task Handle_WhenTeamMissing_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdForUpdateAsync(id, Arg.Any<CancellationToken>()).Returns((Team?)null);

        var result = await _handler.Handle(
            new UploadTeamCrestCommand(id, TeamMother.CrestBytes, TeamMother.CrestContentType),
            CancellationToken.None);

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
        var team = TeamMother.DomainTeam();
        _repository.GetByIdForUpdateAsync(team.Id, Arg.Any<CancellationToken>()).Returns(team);

        await _handler.Handle(
            new UploadTeamCrestCommand(team.Id, TeamMother.CrestBytes, TeamMother.CrestContentType),
            CancellationToken.None);

        await _repository.Received(1).GetByIdForUpdateAsync(team.Id, Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
