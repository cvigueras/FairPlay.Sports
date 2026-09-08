using FairPlay.Sports.Api.Common;
using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Application.Users.Activate;
using FairPlay.Sports.Application.Users.GetAll;
using FairPlay.Sports.Application.Users.GetById;
using FairPlay.Sports.Application.Users.GetPhoto;
using FairPlay.Sports.Application.Users.MoveToTeam;
using FairPlay.Sports.Application.Users.Register;
using FairPlay.Sports.Application.Users.UploadPhoto;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FairPlay.Sports.Api.Users;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class UsersController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    /// <summary>Returns every registered user (without password data).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAllUsersQuery(), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Returns a single user by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetUserByIdQuery(id), cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Registers a new user. Public: this is sign-up.</summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> Register(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(request.UserName, request.Email, request.Password, request.TeamId);
        var result = await _sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ToActionResult(this);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Sets the team the user belongs to.</summary>
    [HttpPut("{id:guid}/team")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> MoveToTeam(Guid id, MoveUserToTeamRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new MoveUserToTeamCommand(id, request.TeamId), cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Activates a user so they can sign in. Users are registered inactive.</summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ActivateUserCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Uploads (or replaces) the user's profile photo.</summary>
    [HttpPost("{id:guid}/photo")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(Domain.Users.User.MaxPhotoBytes + 4096)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadPhoto(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, cancellationToken);

        var command = new UploadUserPhotoCommand(id, buffer.ToArray(), file.ContentType ?? string.Empty);
        var result = await _sender.Send(command, cancellationToken);

        return result.ToActionResult(this);
    }

    /// <summary>Streams the user's profile photo.</summary>
    [HttpGet("{id:guid}/photo")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPhoto(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetUserPhotoQuery(id), cancellationToken);

        if (result.IsSuccess)
        {
            return File(result.Value!.Content, result.Value.ContentType);
        }

        return result.ErrorType == ResultErrorType.NotFound
            ? NotFound(new { error = result.Error })
            : BadRequest(new { error = result.Error });
    }
}
