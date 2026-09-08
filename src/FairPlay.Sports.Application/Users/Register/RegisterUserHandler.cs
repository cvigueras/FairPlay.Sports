using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Domain.Users;
using MediatR;

namespace FairPlay.Sports.Application.Users.Register;

public sealed class RegisterUserHandler(
    IUserRepository respository,
    ITeamRepository teams,
    IPasswordHasher passwordHasher,
    IClock clock) : IRequestHandler<RegisterUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _repository = respository;
    private readonly ITeamRepository _teams = teams;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IClock _clock = clock;

    public async Task<Result<UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.ExistsByEmailAsync(request.Email, cancellationToken))
            return Result<UserDto>.Failure($"Email '{request.Email}' is already registered.");

        if (await _repository.ExistsByUserNameAsync(request.UserName, cancellationToken))
            return Result<UserDto>.Failure($"User name '{request.UserName}' is already taken.");

        if (request.TeamId is { } teamId && !await _teams.ExistsByIdAsync(teamId, cancellationToken))
            return Result<UserDto>.NotFound($"Team '{teamId}' was not found.");

        var user = User.Create(
            Guid.NewGuid(),
            request.UserName,
            request.Email,
            _passwordHasher.Hash(request.Password),
            request.TeamId,
            _clock.UtcNow);

        // A user is only active once they belong to a team.
        if (request.TeamId is not null)
            user.Activate();

        await _repository.AddAsync(user, cancellationToken);

        return Result<UserDto>.Success(UserDto.FromDomain(user));
    }
}
