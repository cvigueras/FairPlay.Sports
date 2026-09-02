using FairPlay.Sports.Api.Auth;
using FairPlay.Sports.Application.Auth;
using FairPlay.Sports.Application.Auth.Login;
using FairPlay.Sports.Application.Auth.Logout;
using FairPlay.Sports.Application.Auth.Refresh;
using FairPlay.Sports.Application.Common;
using FairPlay.Sports.TestSupport.Auth;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace FairPlay.Sports.Api.Tests.Auth;

[TestFixture]
public class AuthControllerTests
{
    private const string RefreshCookieName = "fps_refresh_token";

    private ISender _sender = null!;
    private IWebHostEnvironment _environment = null!;
    private DefaultHttpContext _httpContext = null!;
    private AuthController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _sender = Substitute.For<ISender>();
        _environment = Substitute.For<IWebHostEnvironment>();
        _environment.EnvironmentName.Returns("Production");
        _httpContext = new DefaultHttpContext();
        _controller = new AuthController(_sender, _environment)
        {
            ControllerContext = new ControllerContext { HttpContext = _httpContext }
        };
    }

    private void GivenRequestCookie(string value) =>
        _httpContext.Request.Headers.Cookie = $"{RefreshCookieName}={value}";

    private string? SetCookieHeader() => _httpContext.Response.Headers.SetCookie;

    [Test]
    public async Task Login_MapsRequestOntoCommand()
    {
        var request = AuthRequestMother.LoginRequest();
        _sender.Send(Arg.Any<LoginCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<AuthResultDto>.Success(AuthMother.AuthResult()));

        await _controller.Login(request, CancellationToken.None);

        await _sender.Received(1).Send(
            Arg.Is<LoginCommand>(c => c.Email == request.Email && c.Password == request.Password),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Login_WhenHandlerSucceeds_ReturnsOkWithAccessToken_AndSetsHttpOnlyRefreshCookie()
    {
        _sender.Send(Arg.Any<LoginCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<AuthResultDto>.Success(AuthMother.AuthResult()));

        var response = await _controller.Login(AuthRequestMother.LoginRequest(), CancellationToken.None);

        var ok = response.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var body = ok!.Value as AuthResponse;
        Assert.That(body, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(body!.AccessToken, Is.EqualTo(AuthMother.AccessTokenValue));
            Assert.That(body.User, Is.Not.Null);
            Assert.That(SetCookieHeader(), Does.Contain($"{RefreshCookieName}={AuthMother.RawRefreshToken}"));
            Assert.That(SetCookieHeader(), Does.Contain("httponly"));
            Assert.That(SetCookieHeader(), Does.Contain("secure"));
        });
    }

    [Test]
    public async Task Login_WhenHandlerFails_ReturnsBadRequest_AndSetsNoCookie()
    {
        _sender.Send(Arg.Any<LoginCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<AuthResultDto>.Failure(AuthMother.InvalidCredentials));

        var response = await _controller.Login(AuthRequestMother.LoginRequest(), CancellationToken.None);

        Assert.That(response.Result, Is.InstanceOf<BadRequestObjectResult>());
        Assert.That(SetCookieHeader(), Is.Null.Or.Empty);
    }

    [Test]
    public async Task Refresh_ReadsTheCookie_AndDispatchesCommandWithItsValue()
    {
        GivenRequestCookie("cookie-refresh-token");
        _sender.Send(Arg.Any<RefreshTokenCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<AuthResultDto>.Success(AuthMother.AuthResult()));

        await _controller.Refresh(CancellationToken.None);

        await _sender.Received(1).Send(
            Arg.Is<RefreshTokenCommand>(c => c.RefreshToken == "cookie-refresh-token"),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Refresh_WhenHandlerFails_ReturnsBadRequest()
    {
        GivenRequestCookie("stale");
        _sender.Send(Arg.Any<RefreshTokenCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<AuthResultDto>.Failure(AuthMother.InvalidRefreshToken));

        var response = await _controller.Refresh(CancellationToken.None);

        Assert.That(response.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Refresh_WithNoRefreshCookie_ReturnsUnauthorized_AndDoesNotDispatch()
    {
        var response = await _controller.Refresh(CancellationToken.None);

        Assert.That(response.Result, Is.InstanceOf<UnauthorizedObjectResult>());
        await _sender.DidNotReceive().Send(Arg.Any<RefreshTokenCommand>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Logout_DispatchesCommandWithCookie_ClearsCookie_AndReturnsNoContent()
    {
        GivenRequestCookie("cookie-refresh-token");
        _sender.Send(Arg.Any<LogoutCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var response = await _controller.Logout(CancellationToken.None);

        Assert.That(response, Is.InstanceOf<NoContentResult>());
        await _sender.Received(1).Send(
            Arg.Is<LogoutCommand>(c => c.RefreshToken == "cookie-refresh-token"),
            Arg.Any<CancellationToken>());
        Assert.That(SetCookieHeader(), Does.StartWith($"{RefreshCookieName}=;"));
    }
}
