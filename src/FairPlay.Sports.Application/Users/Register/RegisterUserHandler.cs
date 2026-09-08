using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Domain.Users;
using MediatR;

namespace FairPlay.Sports.Application.Users.Register;

public sealed class RegisterUserHandler(
    IUserRepository respository,
    IPasswordHasher passwordHasher,
    IClock clock) : IRequestHandler<RegisterUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _repository = respository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IClock _clock = clock;

    public async Task<Result<UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.ExistsByEmailAsync(request.Email, cancellationToken))
            return Result<UserDto>.Failure($"Email '{request.Email}' is already registered.");

        if (await _repository.ExistsByUserNameAsync(request.UserName, cancellationToken))
            return Result<UserDto>.Failure($"User name '{request.UserName}' is already taken.");

        var user = User.Create(
            Guid.NewGuid(),
            request.UserName,
            request.Email,
            _passwordHasher.Hash(request.Password),
            request.Team,
            _clock.UtcNow);

        await _repository.AddAsync(user, cancellationToken);

        return Result<UserDto>.Success(UserDto.FromDomain(user));
    }
}
