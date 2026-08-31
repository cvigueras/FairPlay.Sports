using FairPlay.Sports.Application.Auth;
using FairPlay.Sports.Application.Auth.Login;
using FairPlay.Sports.Application.Auth.Logout;
using FairPlay.Sports.Application.Auth.Refresh;
using FairPlay.Sports.Application.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FairPlay.Sports.Api.Auth;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    private const string RefreshTokenCookie = "fps_refresh_token";

    private readonly ISender _sender = sender;

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
        return Authenticated(result);
    }

    
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RefreshTokenCommand(ReadRefreshTokenCookie()), cancellationToken);
        return Authenticated(result);
    }

    
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await _sender.Send(new LogoutCommand(ReadRefreshTokenCookie()), cancellationToken);
        Response.Cookies.Delete(RefreshTokenCookie, RefreshCookieOptions(DateTimeOffset.UnixEpoch));
        return NoContent();
    }

    private ActionResult<AuthResponse> Authenticated(Result<AuthResultDto> result)
    {
        if (!result.IsSuccess)
        {
            return result.ErrorType switch
            {
                ResultErrorType.NotFound => NotFound(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error })
            };
        }

        var auth = result.Value!;
        Response.Cookies.Append(
            RefreshTokenCookie, auth.RefreshToken, RefreshCookieOptions(auth.RefreshTokenExpiresAtUtc));

        return Ok(new AuthResponse(auth.AccessToken, auth.AccessTokenExpiresAtUtc, auth.User));
    }

    private string ReadRefreshTokenCookie() => Request.Cookies[RefreshTokenCookie] ?? string.Empty;

    private static CookieOptions RefreshCookieOptions(DateTimeOffset expires) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = "/",
        Expires = expires
    };
}
