using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Products;
using FairPlay.Sports.Application.Products.GetById;
using FairPlay.Sports.Domain.Products;
using NSubstitute;
using NUnit.Framework;

namespace FairPlay.Sports.Application.Tests.Products.GetById;

[TestFixture]
public class GetProductByIdHandlerTests
{
    private IProductRepository _repository = null!;
    private GetProductByIdHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IProductRepository>();
        _handler = new GetProductByIdHandler(_repository);
    }

    [Test]
    public async Task Handle_WhenProductExists_ReturnsSuccessWithMappedDto()
    {
        var product = new Product(Guid.NewGuid(), "Balón", "desc", 29.99m, 10);
        _repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        var result = await _handler.Handle(new GetProductByIdQuery(product.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Id, Is.EqualTo(product.Id));
            Assert.That(result.Value!.Name, Is.EqualTo("Balón"));
        });
    }

    [Test]
    public async Task Handle_WhenProductDoesNotExist_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Product?)null);

        var result = await _handler.Handle(new GetProductByIdQuery(id));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
            Assert.That(result.Error, Does.Contain(id.ToString()));
        });
    }
}
