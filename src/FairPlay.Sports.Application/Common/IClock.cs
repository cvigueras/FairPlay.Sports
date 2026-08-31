namespace FairPlay.Sports.Application.Common;

/// <summary>
/// Driven port for reading the current UTC time. Handlers depend on this instead of
/// <see cref="DateTime.UtcNow"/> so time-sensitive flows (token expiry, revocation
/// stamps) stay deterministic under test. Implemented in Infrastructure.
/// </summary>
public interface IClock
{
    DateTime UtcNow { get; }
}
