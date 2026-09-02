using FairPlay.Sports.Application.Common;
using FairPlay.Sports.TestSupport.Teams;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Application.Teams.Create;
using FairPlay.Sports.Domain.Teams;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Teams.Create;

/// <summary>
/// Unit tests for <see cref="CreateTeamHandler"/>: the <see cref="ITeamRepository"/> port is
/// mocked, the real <see cref="Team"/> aggregate is built. Scope is the handler's own logic -
/// the uniqueness guard on the name and building the created-team DTO. FluentValidation and the
/// unit-of-work commit are pipeline behaviors and out of scope here. Test data comes from
/// <see cref="TeamMother"/>.
/// </summary>
[TestFixture]
public class CreateTeamHandlerTests
{
    private ITeamRepository _repository = null!;
    private CreateTeamHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<ITeamRepository>();
        _handler = new CreateTeamHandler(_repository);
    }

    [Test]
    public async Task Handle_WhenNameTaken_ReturnsFailure_AndDoesNotPersist()
    {
        _repository.ExistsByNameAsync(TeamMother.Name, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(TeamMother.Command(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.Validation));
            Assert.That(result.Error, Is.EqualTo(TeamMother.NameAlreadyExists));
        });
        await _repository.DidNotReceive().AddAsync(Arg.Any<Team>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenNameFree_PersistsTeam_AndReturnsDto()
    {
        _repository.ExistsByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        var before = DateTime.UtcNow;
        var result = await _handler.Handle(TeamMother.Command(), CancellationToken.None);
        var after = DateTime.UtcNow;

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.Name, Is.EqualTo(TeamMother.Name));
            Assert.That(result.Value!.Coach, Is.EqualTo(TeamMother.Coach));
            Assert.That(result.Value!.City, Is.EqualTo(TeamMother.City));
            Assert.That(result.Value!.Type, Is.EqualTo(TeamMother.DefaultType));
            Assert.That(result.Value!.Division, Is.EqualTo(TeamMother.DefaultDivision));
            Assert.That(result.Value!.Category, Is.EqualTo(TeamMother.DefaultCategory));
            Assert.That(result.Value!.HasCrest, Is.False);
            Assert.That(result.Value!.Active, Is.True);
            Assert.That(result.Value!.Id, Is.Not.EqualTo(Guid.Empty));
        });

        await _repository.Received(1).AddAsync(
            Arg.Is<Team>(team =>
                team.Name == TeamMother.Name &&
                team.Coach == TeamMother.Coach &&
                team.City == TeamMother.City &&
                team.Type == TeamMother.DefaultType &&
                team.Division == TeamMother.DefaultDivision &&
                team.Category == TeamMother.DefaultCategory &&
                !team.HasCrest &&
                team.Active &&
                team.Id != Guid.Empty &&
                team.CreatedAt >= before && team.CreatedAt <= after),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ChecksNameUniquenessBeforePersisting()
    {
        _repository.ExistsByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        await _handler.Handle(TeamMother.Command(), CancellationToken.None);

        await _repository.Received(1).ExistsByNameAsync(TeamMother.Name, Arg.Any<CancellationToken>());
    }
}
