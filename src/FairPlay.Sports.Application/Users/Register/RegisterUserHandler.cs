using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Domain.Users;
using MediatR;

namespace FairPlay.Sports.Application.Users.Register;

public sealed class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserHandler(IUserRepository users, IPasswordHasher passwordHasher)
    {
        _users = users;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken = default)
    {
        if (await _users.ExistsByEmailAsync(request.Email, cancellationToken))
            return Result<UserDto>.Failure($"Email '{request.Email}' is already registered.");

        if (await _users.ExistsByUserNameAsync(request.UserName, cancellationToken))
            return Result<UserDto>.Failure($"User name '{request.UserName}' is already taken.");

        var user = User.Register(
            Guid.NewGuid(),
            request.UserName,
            request.Email,
            _passwordHasher.Hash(request.Password),
            request.Team,
            DateTime.UtcNow);

        await _users.AddAsync(user, cancellationToken);

        // The UnitOfWorkBehavior commits once this handler returns success.
        return Result<UserDto>.Success(UserDto.FromDomain(user));
    }
}
