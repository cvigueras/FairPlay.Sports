using FairPlay.Sports.Application.Common;
using FairPlay.Sports.TestSupport.Teams;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Application.Teams.GetCrest;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Teams.GetCrest;

/// <summary>
/// Unit tests for <see cref="GetTeamCrestHandler"/>: the repository port is mocked. Scope is
/// "return the projected crest, or NotFound when the team has none". Test data comes from
/// <see cref="TeamMother"/>.
/// </summary>
[TestFixture]
public class GetTeamCrestHandlerTests
{
    private ITeamRepository _repository = null!;
    private GetTeamCrestHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<ITeamRepository>();
        _handler = new GetTeamCrestHandler(_repository);
    }

    [Test]
    public async Task Handle_WhenCrestExists_ReturnsIt()
    {
        var id = Guid.NewGuid();
        var crest = new TeamCrest(TeamMother.CrestBytes, TeamMother.CrestContentType);
        _repository.GetCrestAsync(id, Arg.Any<CancellationToken>()).Returns(crest);

        var result = await _handler.Handle(new GetTeamCrestQuery(id), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.Content, Is.EqualTo(TeamMother.CrestBytes));
            Assert.That(result.Value!.ContentType, Is.EqualTo(TeamMother.CrestContentType));
        });
    }

    [Test]
    public async Task Handle_WhenNoCrest_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _repository.GetCrestAsync(id, Arg.Any<CancellationToken>()).Returns((TeamCrest?)null);

        var result = await _handler.Handle(new GetTeamCrestQuery(id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
        });
    }
}
