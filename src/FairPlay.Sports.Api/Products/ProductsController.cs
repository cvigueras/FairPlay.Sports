using FairPlay.Sports.Api.Common;
using FairPlay.Sports.Application.Products;
using FairPlay.Sports.Application.Products.Create;
using FairPlay.Sports.Application.Products.Delete;
using FairPlay.Sports.Application.Products.GetAll;
using FairPlay.Sports.Application.Products.GetById;
using FairPlay.Sports.Application.Products.Update;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FairPlay.Sports.Api.Products;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    /// <summary>Returns the full dummy product catalog.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok((await _sender.Send(new GetAllProductsQuery(), cancellationToken)).Value);
    }

    /// <summary>Returns a single dummy product by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return (await _sender.Send(new GetProductByIdQuery(id), cancellationToken)).ToActionResult(this);
    }

    /// <summary>Creates a new dummy product in the in-memory catalog.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(request.Name, request.Description, request.Price, request.Stock);
        var result = await _sender.Send(command, cancellationToken);

        return !result.IsSuccess
            ? result.ToActionResult(this)
            : (ActionResult<ProductDto>)CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Updates an existing dummy product.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> Update(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        return (await _sender.Send(new UpdateProductCommand(id, request.Name, request.Description, request.Price, request.Stock), cancellationToken)).ToActionResult(this);
    }

    /// <summary>Deletes a dummy product.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return (await _sender.Send(new DeleteProductCommand(id), cancellationToken)).ToActionResult(this);
    }
}
