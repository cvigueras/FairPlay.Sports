using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Users;
using MediatR;

namespace FairPlay.Sports.Application.Auth.Refresh;

public sealed class RefreshTokenHandler(
    IRefreshTokenRepository refreshTokens,
    IUserRepository users,
    IRefreshTokenGenerator refreshTokenGenerator,
    IAuthTokenIssuer tokenIssuer,
    IUnitOfWork unitOfWork,
    IClock clock) : IRequestHandler<RefreshTokenCommand, Result<AuthResultDto>>
{
    private const string InvalidToken = "The refresh token is invalid or has expired.";

    private readonly IRefreshTokenRepository _refreshTokens = refreshTokens;
    private readonly IUserRepository _users = users;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator = refreshTokenGenerator;
    private readonly IAuthTokenIssuer _tokenIssuer = tokenIssuer;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IClock _clock = clock;

    public async Task<Result<AuthResultDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _refreshTokenGenerator.Hash(request.RefreshToken);
        var stored = await _refreshTokens.GetByTokenHashAsync(tokenHash, cancellationToken);
        var now = _clock.UtcNow;

        if (stored is null)
            return Result<AuthResultDto>.Failure(InvalidToken);

        if (!stored.IsActive(now))
        {
            // A token that was already revoked is being presented again: it was replayed
            // or stolen. Burn every still-active token for that user so an attacker who
            // holds one cannot ride it back in.
            if (stored.RevokedAtUtc is not null)
            {
                foreach (var active in await _refreshTokens.GetActiveByUserIdAsync(stored.UserId, now, cancellationToken))
                    active.Revoke(now);

                await PersistFailedSideEffectsAsync(cancellationToken);
            }

            return Result<AuthResultDto>.Failure(InvalidToken);
        }

        var user = await _users.GetByIdAsync(stored.UserId, cancellationToken);
        if (user is null)
        {
            stored.Revoke(now);
            await PersistFailedSideEffectsAsync(cancellationToken);
            return Result<AuthResultDto>.Failure(InvalidToken);
        }

        var issued = await _tokenIssuer.IssueAsync(user, cancellationToken);
        stored.Revoke(now, issued.RefreshTokenId);

        return Result<AuthResultDto>.Success(issued.Result);
    }

    // The happy path is committed by UnitOfWorkBehavior. Revocations decided on a
    // failure path are not (the behavior skips a failed Result on purpose), yet they
    // must be durable, so they are flushed explicitly here.
    private Task PersistFailedSideEffectsAsync(CancellationToken cancellationToken) =>
        _unitOfWork.SaveChangesAsync(cancellationToken);
}
