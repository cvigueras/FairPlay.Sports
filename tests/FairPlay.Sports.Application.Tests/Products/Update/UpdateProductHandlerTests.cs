using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Products;
using FairPlay.Sports.Application.Products.Update;
using FairPlay.Sports.Domain.Products;
using NSubstitute;
using NUnit.Framework;

namespace FairPlay.Sports.Application.Tests.Products.Update;

/// <summary>
/// The handler no longer validates: FluentValidation now runs in the MediatR
/// <c>ValidationBehavior</c> pipeline step (see <c>ValidationBehaviorTests</c>).
/// These tests cover only the handler's own job - look up, mutate and persist.
/// </summary>
[TestFixture]
public class UpdateProductHandlerTests
{
    private IProductRepository _repository = null!;
    private UpdateProductHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IProductRepository>();
        _handler = new UpdateProductHandler(_repository);
    }

    [Test]
    public async Task Handle_WhenProductExists_UpdatesAndReturnsSuccess()
    {
        var existing = new Product(Guid.NewGuid(), "Old name", "Old desc", 10m, 5);
        _repository.GetByIdAsync(existing.Id, Arg.Any<CancellationToken>()).Returns(existing);

        var command = new UpdateProductCommand(existing.Id, "New name", "New desc", 20m, 15);

        var result = await _handler.Handle(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Name, Is.EqualTo("New name"));
            Assert.That(result.Value!.Price, Is.EqualTo(20m));
            Assert.That(result.Value!.Stock, Is.EqualTo(15));
        });
        await _repository.Received(1).UpdateAsync(existing, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenProductDoesNotExist_ReturnsNotFound()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var command = new UpdateProductCommand(Guid.NewGuid(), "New name", "desc", 20m, 15);

        var result = await _handler.Handle(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
        });
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }
}
