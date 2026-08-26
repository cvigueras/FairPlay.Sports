using FairPlay.Sports.Api.Products;
using FairPlay.Sports.Application.Abstractions;
using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Products;
using FairPlay.Sports.Application.Products.Create;
using FairPlay.Sports.Application.Products.Delete;
using FairPlay.Sports.Application.Products.GetAll;
using FairPlay.Sports.Application.Products.GetById;
using FairPlay.Sports.Application.Products.Update;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;

namespace FairPlay.Sports.Api.Tests.Products;

/// <summary>
/// Unit tests for ProductsController in isolation: every application handler is mocked,
/// so these tests verify only the controller's own responsibility - translating
/// Result/Result&lt;T&gt; outcomes into the correct HTTP responses.
/// </summary>
[TestFixture]
public class ProductsControllerTests
{
    private IQueryHandler<GetAllProductsQuery, IReadOnlyList<ProductDto>> _getAllHandler = null!;
    private IQueryHandler<GetProductByIdQuery, ProductDto> _getByIdHandler = null!;
    private ICommandHandler<CreateProductCommand, ProductDto> _createHandler = null!;
    private ICommandHandler<UpdateProductCommand, ProductDto> _updateHandler = null!;
    private ICommandHandler<DeleteProductCommand> _deleteHandler = null!;
    private ProductsController _controller = null!;

    private static ProductDto SampleDto(Guid? id = null) =>
        new(id ?? Guid.NewGuid(), "Balón", "desc", 29.99m, 10);

    [SetUp]
    public void SetUp()
    {
        _getAllHandler = Substitute.For<IQueryHandler<GetAllProductsQuery, IReadOnlyList<ProductDto>>>();
        _getByIdHandler = Substitute.For<IQueryHandler<GetProductByIdQuery, ProductDto>>();
        _createHandler = Substitute.For<ICommandHandler<CreateProductCommand, ProductDto>>();
        _updateHandler = Substitute.For<ICommandHandler<UpdateProductCommand, ProductDto>>();
        _deleteHandler = Substitute.For<ICommandHandler<DeleteProductCommand>>();

        _controller = new ProductsController(
            _getAllHandler, _getByIdHandler, _createHandler, _updateHandler, _deleteHandler);
    }

    [Test]
    public async Task GetAll_ReturnsOkWithHandlerValue()
    {
        IReadOnlyList<ProductDto> dtos = new List<ProductDto> { SampleDto() };
        _getAllHandler.Handle(Arg.Any<GetAllProductsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<ProductDto>>.Success(dtos));

        var response = await _controller.GetAll(CancellationToken.None);

        var okResult = response.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.SameAs(dtos));
    }

    [Test]
    public async Task GetById_WhenHandlerSucceeds_ReturnsOkWithDto()
    {
        var dto = SampleDto();
        _getByIdHandler.Handle(Arg.Any<GetProductByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<ProductDto>.Success(dto));

        var response = await _controller.GetById(dto.Id, CancellationToken.None);

        var okResult = response.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.SameAs(dto));
    }

    [Test]
    public async Task GetById_WhenHandlerReturnsNotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _getByIdHandler.Handle(Arg.Any<GetProductByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<ProductDto>.NotFound($"Product '{id}' was not found."));

        var response = await _controller.GetById(id, CancellationToken.None);

        Assert.That(response.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task Create_WhenHandlerSucceeds_ReturnsCreatedAtActionPointingToGetById()
    {
        var dto = SampleDto();
        _createHandler.Handle(Arg.Any<CreateProductCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<ProductDto>.Success(dto));
        var request = new CreateProductRequest(dto.Name, dto.Description, dto.Price, dto.Stock);

        var response = await _controller.Create(request, CancellationToken.None);

        var createdResult = response.Result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(createdResult!.ActionName, Is.EqualTo(nameof(ProductsController.GetById)));
            Assert.That(createdResult!.RouteValues!["id"], Is.EqualTo(dto.Id));
            Assert.That(createdResult!.Value, Is.SameAs(dto));
        });
    }

    [Test]
    public async Task Create_WhenHandlerFailsValidation_ReturnsBadRequest()
    {
        _createHandler.Handle(Arg.Any<CreateProductCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<ProductDto>.Failure("Name is required"));
        var request = new CreateProductRequest("", "desc", 1m, 1);

        var response = await _controller.Create(request, CancellationToken.None);

        Assert.That(response.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Update_WhenHandlerSucceeds_ReturnsOkWithUpdatedDto()
    {
        var dto = SampleDto();
        _updateHandler.Handle(Arg.Any<UpdateProductCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<ProductDto>.Success(dto));
        var request = new UpdateProductRequest(dto.Name, dto.Description, dto.Price, dto.Stock);

        var response = await _controller.Update(dto.Id, request, CancellationToken.None);

        var okResult = response.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.SameAs(dto));
    }

    [Test]
    public async Task Update_WhenHandlerReturnsNotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _updateHandler.Handle(Arg.Any<UpdateProductCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<ProductDto>.NotFound($"Product '{id}' was not found."));
        var request = new UpdateProductRequest("Name", "desc", 1m, 1);

        var response = await _controller.Update(id, request, CancellationToken.None);

        Assert.That(response.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task Delete_WhenHandlerSucceeds_ReturnsNoContent()
    {
        _deleteHandler.Handle(Arg.Any<DeleteProductCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var response = await _controller.Delete(Guid.NewGuid(), CancellationToken.None);

        Assert.That(response, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task Delete_WhenHandlerReturnsNotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _deleteHandler.Handle(Arg.Any<DeleteProductCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.NotFound($"Product '{id}' was not found."));

        var response = await _controller.Delete(id, CancellationToken.None);

        Assert.That(response, Is.InstanceOf<NotFoundObjectResult>());
    }
}
