using FairPlay.Sports.Application.Products;
using FairPlay.Sports.Application.Products.Create;
using FairPlay.Sports.Domain.Products;
using NSubstitute;
using NUnit.Framework;

namespace FairPlay.Sports.Application.Tests.Products.Create;

/// <summary>
/// The handler no longer validates: FluentValidation now runs in the MediatR
/// <c>ValidationBehavior</c> pipeline step (see <c>ValidationBehaviorTests</c>).
/// These tests cover only the handler's own job - create and persist.
/// </summary>
[TestFixture]
public class CreateProductHandlerTests
{
    private IProductRepository _repository = null!;
    private CreateProductHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IProductRepository>();
        _handler = new CreateProductHandler(_repository);
    }

    [Test]
    public async Task Handle_WithValidCommand_ReturnsSuccessWithMappedDto()
    {
        var command = new CreateProductCommand("Balón", "desc", 29.99m, 10);

        var result = await _handler.Handle(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Name, Is.EqualTo("Balón"));
            Assert.That(result.Value!.Price, Is.EqualTo(29.99m));
            Assert.That(result.Value!.Stock, Is.EqualTo(10));
            Assert.That(result.Value!.Id, Is.Not.EqualTo(Guid.Empty));
        });
    }

    [Test]
    public async Task Handle_WithValidCommand_PersistsProductThroughRepository()
    {
        var command = new CreateProductCommand("Balón", "desc", 29.99m, 10);

        await _handler.Handle(command);

        await _repository.Received(1).AddAsync(
            Arg.Is<Product>(p => p.Name == "Balón" && p.Price == 29.99m),
            Arg.Any<CancellationToken>());
    }
}
