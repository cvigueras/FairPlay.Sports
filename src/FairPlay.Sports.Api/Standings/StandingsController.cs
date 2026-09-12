using FairPlay.Sports.Api.Common;
using FairPlay.Sports.Application.Common.Querying;
using FairPlay.Sports.Application.Standings;
using FairPlay.Sports.Application.Standings.Create;
using FairPlay.Sports.Application.Standings.Delete;
using FairPlay.Sports.Application.Standings.GetById;
using FairPlay.Sports.Application.Standings.GetPage;
using FairPlay.Sports.Application.Standings.Update;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FairPlay.Sports.Api.Standings;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class StandingsController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<StandingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<StandingDto>>> GetPage(
        [FromQuery] GetStandingsPageRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetStandingsPageQuery(
            request.Page,
            request.PageSize,
            request.Sort,
            new StandingFilter(request.TeamId, request.Type, request.Division, request.Category));

        var result = await _sender.Send(query, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StandingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StandingDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetStandingByIdQuery(id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [ProducesResponseType(typeof(StandingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StandingDto>> Create(CreateStandingRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateStandingCommand(
            request.TeamId,
            request.Points,
            request.Played,
            request.Won,
            request.Drawn,
            request.Lost,
            request.GoalsFor,
            request.GoalsAgainst);
        var result = await _sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ToActionResult(this);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(StandingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StandingDto>> Update(
        Guid id,
        UpdateStandingRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateStandingCommand(
            id,
            request.Points,
            request.Played,
            request.Won,
            request.Drawn,
            request.Lost,
            request.GoalsFor,
            request.GoalsAgainst);

        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteStandingCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }
}
