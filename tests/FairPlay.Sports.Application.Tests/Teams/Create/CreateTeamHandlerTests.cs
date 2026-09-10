using FairPlay.Sports.Application.Common;
using FairPlay.Sports.TestSupport.Teams;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Application.Teams.Create;
using FairPlay.Sports.Domain.Teams;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Teams.Create;

[TestFixture]
public class CreateTeamHandlerTests
{
    private static readonly DateTime Now = new(2026, 9, 2, 12, 0, 0, DateTimeKind.Utc);

    private ITeamRepository _repository = null!;
    private IClock _clock = null!;
    private CreateTeamHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<ITeamRepository>();
        _clock = Substitute.For<IClock>();
        _clock.UtcNow.Returns(Now);
        _handler = new CreateTeamHandler(_repository, _clock);
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

        var result = await _handler.Handle(TeamMother.Command(), CancellationToken.None);

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
            Assert.That(result.Value!.Active, Is.False);
            Assert.That(result.Value!.CreatedAt, Is.EqualTo(Now));
            Assert.That(result.Value!.Id, Is.Not.EqualTo(Guid.Empty));
        });

        await _repository.Received(1).AddAsync(
            Arg.Is<Team>(team =>
                team.Name == TeamMother.Name &&
                team.Coach == TeamMother.Coach &&
                team.City == TeamMother.City &&
                team.Classification.Type == TeamMother.DefaultType &&
                team.Classification.Division == TeamMother.DefaultDivision &&
                team.Classification.Category == TeamMother.DefaultCategory &&
                !team.HasCrest &&
                !team.Active &&
                team.Id != Guid.Empty &&
                team.CreatedAt == Now),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ChecksNameUniquenessBeforePersisting()
    {
        _repository.ExistsByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        await _handler.Handle(TeamMother.Command(), CancellationToken.None);

        await _repository.Received(1).ExistsByNameAsync(TeamMother.Name, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WithProfileFields_MapsThemOntoTheAggregateAndDto()
    {
        _repository.ExistsByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        Team? persisted = null;
        await _repository.AddAsync(Arg.Do<Team>(t => persisted = t), Arg.Any<CancellationToken>());

        var result = await _handler.Handle(TeamMother.CommandWithProfile(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.ShortName, Is.EqualTo(TeamMother.ShortName));
            Assert.That(result.Value!.FoundedYear, Is.EqualTo(TeamMother.FoundedYear));
            Assert.That(result.Value!.VenueName, Is.EqualTo(TeamMother.VenueName));
            Assert.That(result.Value!.VenueAddress, Is.EqualTo(TeamMother.VenueAddress));
            Assert.That(result.Value!.VenueSurface, Is.EqualTo(TeamMother.VenueSurface));
            Assert.That(result.Value!.VenueMapsUrl, Is.EqualTo(TeamMother.VenueMapsUrl));
            Assert.That(result.Value!.ColorPrimary, Is.EqualTo(TeamMother.ColorPrimary));
            Assert.That(result.Value!.ColorSecondary, Is.EqualTo(TeamMother.ColorSecondary));
            Assert.That(result.Value!.ContactEmail, Is.EqualTo(TeamMother.ContactEmail));
            Assert.That(result.Value!.ContactPhone, Is.EqualTo(TeamMother.ContactPhone));
            Assert.That(result.Value!.Website, Is.EqualTo(TeamMother.Website));
        });
        Assert.Multiple(() =>
        {
            Assert.That(persisted!.HomeVenue!.Surface, Is.EqualTo(TeamMother.VenueSurface));
            Assert.That(persisted!.Colors!.Primary, Is.EqualTo(TeamMother.ColorPrimary));
        });
    }
}
