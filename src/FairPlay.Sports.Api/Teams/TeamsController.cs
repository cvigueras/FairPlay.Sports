using FairPlay.Sports.Api.Common;
using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Application.Teams.Create;
using FairPlay.Sports.Application.Teams.GetAll;
using FairPlay.Sports.Application.Teams.GetById;
using FairPlay.Sports.Application.Teams.GetCrest;
using FairPlay.Sports.Application.Teams.UploadCrest;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FairPlay.Sports.Api.Teams;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TeamsController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TeamDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TeamDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAllTeamsQuery(), cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTeamByIdQuery(id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TeamDto>> Create(CreateTeamRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateTeamCommand(
            request.Name,
            request.Coach,
            request.City,
            request.Type,
            request.Division,
            request.Category);
        var result = await _sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ToActionResult(this);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPost("{id:guid}/crest")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(Domain.Teams.Team.MaxCrestBytes + 4096)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadCrest(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, cancellationToken);

        var command = new UploadTeamCrestCommand(id, buffer.ToArray(), file.ContentType ?? string.Empty);
        var result = await _sender.Send(command, cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpGet("{id:guid}/crest")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCrest(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTeamCrestQuery(id), cancellationToken);

        if (result.IsSuccess)
        {
            return File(result.Value!.Content, result.Value.ContentType);
        }

        return result.ErrorType == ResultErrorType.NotFound
            ? NotFound(new { error = result.Error })
            : BadRequest(new { error = result.Error });
    }
}
