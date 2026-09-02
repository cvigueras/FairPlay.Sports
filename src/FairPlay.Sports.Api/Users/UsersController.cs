using FairPlay.Sports.Api.Common;
using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Application.Users.Activate;
using FairPlay.Sports.Application.Users.GetAll;
using FairPlay.Sports.Application.Users.GetById;
using FairPlay.Sports.Application.Users.Register;
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

    /// <summary>Registers a new user and persists it to SQL Server. Public: this is sign-up.</summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> Register(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(request.UserName, request.Email, request.Password, request.Team);
        var result = await _sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ToActionResult(this);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
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
}
