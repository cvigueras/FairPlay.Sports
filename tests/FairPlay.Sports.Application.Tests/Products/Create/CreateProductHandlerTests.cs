using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Products;
using FairPlay.Sports.Application.Products.Create;
using FairPlay.Sports.Domain.Products;
using FluentValidation;
using NSubstitute;
using NUnit.Framework;

namespace FairPlay.Sports.Application.Tests.Products.Create;

[TestFixture]
public class CreateProductHandlerTests
{
    private IProductRepository _repository = null!;
    private IValidator<CreateProductCommand> _validator = null!;
    private CreateProductHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IProductRepository>();
        _validator = new CreateProductValidator();
        _handler = new CreateProductHandler(_repository, _validator);
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

    [Test]
    public async Task Handle_WithEmptyName_ReturnsValidationFailureAndDoesNotCallRepository()
    {
        var command = new CreateProductCommand("", "desc", 29.99m, 10);

        var result = await _handler.Handle(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.Validation));
            Assert.That(result.Error, Is.Not.Null.And.Not.Empty);
        });
        await _repository.DidNotReceive().AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WithNegativePrice_ReturnsValidationFailure()
    {
        var command = new CreateProductCommand("Balón", "desc", -1m, 10);

        var result = await _handler.Handle(command);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.Validation));
    }

    [Test]
    public async Task Handle_WithNegativeStock_ReturnsValidationFailure()
    {
        var command = new CreateProductCommand("Balón", "desc", 1m, -1);

        var result = await _handler.Handle(command);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.Validation));
    }
}
