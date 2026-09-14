using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Users.UpdateProfile;

public sealed class UpdateUserProfileHandler(IUserRepository users)
    : IRequestHandler<UpdateUserProfileCommand, Result<UserDto>>
{
    private readonly IUserRepository _users = users;

    public async Task<Result<UserDto>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdForUpdateAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result<UserDto>.NotFound($"User '{request.UserId}' was not found.");

        var newUserName = request.UserName.Trim();
        if (!string.Equals(newUserName, user.UserName, StringComparison.Ordinal)
            && await _users.ExistsByUserNameAsync(newUserName, cancellationToken))
            return Result<UserDto>.Failure($"User name '{newUserName}' is already taken.");

        var newEmail = request.Email.Trim();
        if (!string.Equals(newEmail, user.Email, StringComparison.OrdinalIgnoreCase)
            && await _users.ExistsByEmailAsync(newEmail, cancellationToken))
            return Result<UserDto>.Failure($"Email '{newEmail}' is already registered.");

        user.UpdateProfile(request.UserName, request.Email);

        return Result<UserDto>.Success(UserDto.FromDomain(user));
    }
}
