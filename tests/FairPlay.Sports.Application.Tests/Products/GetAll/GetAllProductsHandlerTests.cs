using FairPlay.Sports.Application.Products;
using FairPlay.Sports.Application.Products.GetAll;
using FairPlay.Sports.Domain.Products;
using NSubstitute;
using NUnit.Framework;

namespace FairPlay.Sports.Application.Tests.Products.GetAll;

[TestFixture]
public class GetAllProductsHandlerTests
{
    private IProductRepository _repository = null!;
    private GetAllProductsHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IProductRepository>();
        _handler = new GetAllProductsHandler(_repository);
    }

    [Test]
    public async Task Handle_WhenRepositoryHasProducts_ReturnsAllMappedAsDtos()
    {
        IReadOnlyList<Product> products = new List<Product>
        {
            new(Guid.NewGuid(), "Balón", "desc", 29.99m, 10),
            new(Guid.NewGuid(), "Raqueta", "desc", 149.50m, 5)
        };
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(products);

        var result = await _handler.Handle(new GetAllProductsQuery());

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Has.Count.EqualTo(2));
            Assert.That(result.Value!.Select(p => p.Name), Is.EquivalentTo(new[] { "Balón", "Raqueta" }));
        });
    }

    [Test]
    public async Task Handle_WhenRepositoryIsEmpty_ReturnsSuccessWithEmptyList()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Product>());

        var result = await _handler.Handle(new GetAllProductsQuery());

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Empty);
        });
    }
}
