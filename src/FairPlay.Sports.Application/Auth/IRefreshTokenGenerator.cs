namespace FairPlay.Sports.Application.Auth;

/// <summary>
/// Driven port for the opaque refresh-token secret: creates a fresh cryptographically
/// random value and derives the one-way hash that is the only form ever persisted.
/// Implemented in Infrastructure.
/// </summary>
public interface IRefreshTokenGenerator
{
    /// <summary>A new URL-safe random token handed to the client exactly once.</summary>
    string NewToken();

    /// <summary>Deterministic hash of <paramref name="token"/> for storage and lookup.</summary>
    string Hash(string token);
}
