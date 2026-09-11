using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Standings;
using FairPlay.Sports.Application.Standings.Delete;
using FairPlay.Sports.Domain.Standings;
using FairPlay.Sports.TestSupport.Standings;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Standings.Delete;

[TestFixture]
public class DeleteStandingHandlerTests
{
    private IStandingRepository _repository = null!;
    private DeleteStandingHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IStandingRepository>();
        _handler = new DeleteStandingHandler(_repository);
    }

    [Test]
    public async Task Handle_WhenMissing_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdForUpdateAsync(id, Arg.Any<CancellationToken>()).Returns((Standing?)null);

        var result = await _handler.Handle(new DeleteStandingCommand(id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
        });
        await _repository.DidNotReceive().RemoveAsync(Arg.Any<Standing>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenPresent_RemovesStanding_AndReturnsSuccess()
    {
        var standing = StandingMother.DomainStanding();
        _repository.GetByIdForUpdateAsync(standing.Id, Arg.Any<CancellationToken>()).Returns(standing);

        var result = await _handler.Handle(new DeleteStandingCommand(standing.Id), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        await _repository.Received(1).RemoveAsync(standing, Arg.Any<CancellationToken>());
    }
}
