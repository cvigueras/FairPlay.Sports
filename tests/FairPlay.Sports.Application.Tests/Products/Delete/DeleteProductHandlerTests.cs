using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Products;
using FairPlay.Sports.Application.Products.Delete;
using NSubstitute;
using NUnit.Framework;

namespace FairPlay.Sports.Application.Tests.Products.Delete;

[TestFixture]
public class DeleteProductHandlerTests
{
    private IProductRepository _repository = null!;
    private DeleteProductHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IProductRepository>();
        _handler = new DeleteProductHandler(_repository);
    }

    [Test]
    public async Task Handle_WhenProductExists_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        _repository.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(new DeleteProductCommand(id));

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task Handle_WhenProductDoesNotExist_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _repository.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(new DeleteProductCommand(id));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
        });
    }
}
