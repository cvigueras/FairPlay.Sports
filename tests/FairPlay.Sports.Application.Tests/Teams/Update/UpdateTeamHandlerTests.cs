using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Application.Teams.Update;
using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.TestSupport.Teams;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Teams.Update;

[TestFixture]
public class UpdateTeamHandlerTests
{
    private ITeamRepository _repository = null!;
    private UpdateTeamHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<ITeamRepository>();
        _handler = new UpdateTeamHandler(_repository);
    }

    [Test]
    public async Task Handle_WhenTeamMissing_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdForUpdateAsync(id, Arg.Any<CancellationToken>()).Returns((Team?)null);

        var result = await _handler.Handle(TeamMother.UpdateCommand(id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
            Assert.That(result.Error, Does.Contain(id.ToString()));
        });
    }

    [Test]
    public async Task Handle_WhenTeamExists_MutatesTheTrackedAggregate_AndReturnsDto()
    {
        var team = TeamMother.DomainTeam(city: "Sevilla");
        _repository.GetByIdForUpdateAsync(team.Id, Arg.Any<CancellationToken>()).Returns(team);

        var command = TeamMother.UpdateCommand(team.Id) with { City = "Cartagena" };
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(team.City, Is.EqualTo("Cartagena"));
            Assert.That(team.ShortName, Is.EqualTo(TeamMother.ShortName));
            Assert.That(team.HomeVenue!.Surface, Is.EqualTo(TeamMother.VenueSurface));
            Assert.That(team.Colors!.Primary, Is.EqualTo(TeamMother.ColorPrimary));
            Assert.That(result.Value!.City, Is.EqualTo("Cartagena"));
            Assert.That(result.Value!.VenueMapsUrl, Is.EqualTo(TeamMother.VenueMapsUrl));
        });
    }

    [Test]
    public async Task Handle_WhenRenamingToAnExistingName_ReturnsFailure()
    {
        var team = TeamMother.DomainTeam();
        _repository.GetByIdForUpdateAsync(team.Id, Arg.Any<CancellationToken>()).Returns(team);
        _repository.ExistsByNameAsync("Taken FC", Arg.Any<CancellationToken>()).Returns(true);

        var command = TeamMother.UpdateCommand(team.Id) with { Name = "Taken FC" };
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.Validation));
        });
    }

    [Test]
    public async Task Handle_WhenNameUnchanged_DoesNotCheckUniqueness()
    {
        var team = TeamMother.DomainTeam();
        _repository.GetByIdForUpdateAsync(team.Id, Arg.Any<CancellationToken>()).Returns(team);

        await _handler.Handle(TeamMother.UpdateCommand(team.Id), CancellationToken.None);

        await _repository.DidNotReceive().ExistsByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_LoadsTheTeamThroughTheTrackedGetter()
    {
        var team = TeamMother.DomainTeam();
        _repository.GetByIdForUpdateAsync(team.Id, Arg.Any<CancellationToken>()).Returns(team);

        await _handler.Handle(TeamMother.UpdateCommand(team.Id), CancellationToken.None);

        await _repository.Received(1).GetByIdForUpdateAsync(team.Id, Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
