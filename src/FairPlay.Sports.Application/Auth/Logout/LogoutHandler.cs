using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Auth.Logout;

/// <summary>
/// Revokes the caller's current refresh token. Idempotent: an unknown, already-revoked
/// or expired token still returns success, so a client can always "log out" cleanly.
/// The revocation is committed by <c>UnitOfWorkBehavior</c> because the command succeeds.
/// </summary>
public sealed class LogoutHandler(
    IRefreshTokenRepository refreshTokens,
    IRefreshTokenGenerator refreshTokenGenerator,
    IClock clock) : IRequestHandler<LogoutCommand, Result>
{
    private readonly IRefreshTokenRepository _refreshTokens = refreshTokens;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator = refreshTokenGenerator;
    private readonly IClock _clock = clock;

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var stored = await _refreshTokens.GetByTokenHashAsync(
            _refreshTokenGenerator.Hash(request.RefreshToken), cancellationToken);

        stored?.Revoke(_clock.UtcNow);

        return Result.Success();
    }
}
