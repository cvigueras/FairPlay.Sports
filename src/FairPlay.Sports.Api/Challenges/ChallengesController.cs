using FairPlay.Sports.Api.Common;
using FairPlay.Sports.Application.Challenges;
using FairPlay.Sports.Application.Challenges.Accept;
using FairPlay.Sports.Application.Challenges.GetTeamChallenges;
using FairPlay.Sports.Application.Challenges.Reject;
using FairPlay.Sports.Application.Challenges.Send;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FairPlay.Sports.Api.Challenges;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ChallengesController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    /// <summary>Every challenge a team sent or received.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ChallengeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ChallengeDto>>> GetForTeam(
        [FromQuery] Guid teamId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTeamChallengesQuery(teamId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ChallengeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChallengeDto>> Send(SendChallengeRequest request, CancellationToken cancellationToken)
    {
        var command = new SendChallengeCommand(
            request.ChallengerTeamId,
            request.ChallengedTeamId,
            request.VenueTeamId,
            request.MatchDate,
            request.Message,
            User.GetUserId(),
            request.ChallengerKitPreference);

        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/accept")]
    [ProducesResponseType(typeof(ChallengeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChallengeDto>> Accept(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AcceptChallengeCommand(id, User.GetUserId()), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(ChallengeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChallengeDto>> Reject(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RejectChallengeCommand(id, User.GetUserId()), cancellationToken);
        return result.ToActionResult(this);
    }
}
