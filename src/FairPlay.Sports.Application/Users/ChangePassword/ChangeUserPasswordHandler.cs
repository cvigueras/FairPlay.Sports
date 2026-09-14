using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Users.ChangePassword;

public sealed class ChangeUserPasswordHandler(IUserRepository users, IPasswordHasher passwordHasher)
    : IRequestHandler<ChangeUserPasswordCommand, Result>
{
    private readonly IUserRepository _users = users;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    public async Task<Result> Handle(ChangeUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdForUpdateAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.NotFound($"User '{request.UserId}' was not found.");

        if (!_passwordHasher.Verify(user.PasswordHash, request.CurrentPassword))
            return Result.Failure("The current password is incorrect.");

        user.ChangePasswordHash(_passwordHasher.Hash(request.NewPassword));

        return Result.Success();
    }
}
