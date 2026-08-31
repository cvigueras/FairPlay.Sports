using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Users;
using MediatR;

namespace FairPlay.Sports.Application.Auth.Login;

public sealed class LoginHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IAuthTokenIssuer tokenIssuer) : IRequestHandler<LoginCommand, Result<AuthResultDto>>
{
    private const string InvalidCredentials = "Email or password is incorrect.";

    private readonly IUserRepository _users = users;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IAuthTokenIssuer _tokenIssuer = tokenIssuer;

    public async Task<Result<AuthResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByEmailAsync(request.Email, cancellationToken);

        // One error for every failure mode - unknown email, wrong password, deactivated
        // account - so the endpoint never confirms which addresses are registered.
        if (user is null || !user.Active || !_passwordHasher.Verify(user.PasswordHash, request.Password))
            return Result<AuthResultDto>.Failure(InvalidCredentials);

        var issued = await _tokenIssuer.IssueAsync(user, cancellationToken);
        return Result<AuthResultDto>.Success(issued.Result);
    }
}
