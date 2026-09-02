using FairPlay.Sports.Application.Common;
using FairPlay.Sports.TestSupport.Teams;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Application.Teams.GetById;
using FairPlay.Sports.Domain.Teams;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Teams.GetById;

/// <summary>
/// Unit tests for <see cref="GetTeamByIdHandler"/>: the repository port is mocked, the real
/// <c>TeamDto.FromDomain</c> mapping runs. Scope is the handler's own logic - look up by id and
/// translate "missing" into a NotFound Result. Test data comes from <see cref="TeamMother"/>.
/// </summary>
[TestFixture]
public class GetTeamByIdHandlerTests
{
    private ITeamRepository _repository = null!;
    private GetTeamByIdHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<ITeamRepository>();
        _handler = new GetTeamByIdHandler(_repository);
    }

    [Test]
    public async Task Handle_WhenTeamExists_ReturnsSuccessWithMappedDto()
    {
        var team = TeamMother.DomainTeam();
        _repository.GetByIdAsync(team.Id, Arg.Any<CancellationToken>()).Returns(team);

        var result = await _handler.Handle(new GetTeamByIdQuery(team.Id), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.Id, Is.EqualTo(team.Id));
            Assert.That(result.Value!.Name, Is.EqualTo(team.Name));
            Assert.That(result.Value!.Coach, Is.EqualTo(team.Coach));
            Assert.That(result.Value!.City, Is.EqualTo(team.City));
            Assert.That(result.Value!.Type, Is.EqualTo(team.Type));
            Assert.That(result.Value!.Division, Is.EqualTo(team.Division));
            Assert.That(result.Value!.Category, Is.EqualTo(team.Category));
            Assert.That(result.Value!.HasCrest, Is.EqualTo(team.HasCrest));
            Assert.That(result.Value!.Active, Is.EqualTo(team.Active));
        });
    }

    [Test]
    public async Task Handle_WhenTeamHasCrest_ReportsHasCrestTrue()
    {
        var team = TeamMother.DomainTeamWithCrest();
        _repository.GetByIdAsync(team.Id, Arg.Any<CancellationToken>()).Returns(team);

        var result = await _handler.Handle(new GetTeamByIdQuery(team.Id), CancellationToken.None);

        Assert.That(result.Value!.HasCrest, Is.True);
    }

    [Test]
    public async Task Handle_WhenTeamMissing_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Team?)null);

        var result = await _handler.Handle(new GetTeamByIdQuery(id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
            Assert.That(result.Error, Does.Contain(id.ToString()));
        });
    }

    [Test]
    public async Task Handle_QueriesRepositoryWithRequestedId()
    {
        var id = Guid.NewGuid();

        await _handler.Handle(new GetTeamByIdQuery(id), CancellationToken.None);

        await _repository.Received(1).GetByIdAsync(id, Arg.Any<CancellationToken>());
    }
}
