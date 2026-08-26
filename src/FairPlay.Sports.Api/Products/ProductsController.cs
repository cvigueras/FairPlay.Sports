using FairPlay.Sports.Api.Common;
using FairPlay.Sports.Application.Abstractions;
using FairPlay.Sports.Application.Products;
using FairPlay.Sports.Application.Products.Create;
using FairPlay.Sports.Application.Products.Delete;
using FairPlay.Sports.Application.Products.GetAll;
using FairPlay.Sports.Application.Products.GetById;
using FairPlay.Sports.Application.Products.Update;
using Microsoft.AspNetCore.Mvc;

namespace FairPlay.Sports.Api.Products;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController : ControllerBase
{
    private readonly IQueryHandler<GetAllProductsQuery, IReadOnlyList<ProductDto>> _getAllHandler;
    private readonly IQueryHandler<GetProductByIdQuery, ProductDto> _getByIdHandler;
    private readonly ICommandHandler<CreateProductCommand, ProductDto> _createHandler;
    private readonly ICommandHandler<UpdateProductCommand, ProductDto> _updateHandler;
    private readonly ICommandHandler<DeleteProductCommand> _deleteHandler;

    public ProductsController(
        IQueryHandler<GetAllProductsQuery, IReadOnlyList<ProductDto>> getAllHandler,
        IQueryHandler<GetProductByIdQuery, ProductDto> getByIdHandler,
        ICommandHandler<CreateProductCommand, ProductDto> createHandler,
        ICommandHandler<UpdateProductCommand, ProductDto> updateHandler,
        ICommandHandler<DeleteProductCommand> deleteHandler)
    {
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
    }

    /// <summary>Returns the full dummy product catalog.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _getAllHandler.Handle(new GetAllProductsQuery(), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Returns a single dummy product by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getByIdHandler.Handle(new GetProductByIdQuery(id), cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Creates a new dummy product in the in-memory catalog.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(request.Name, request.Description, request.Price, request.Stock);
        var result = await _createHandler.Handle(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ToActionResult(this);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Updates an existing dummy product.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> Update(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(id, request.Name, request.Description, request.Price, request.Stock);
        var result = await _updateHandler.Handle(command, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Deletes a dummy product.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _deleteHandler.Handle(new DeleteProductCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }
}
