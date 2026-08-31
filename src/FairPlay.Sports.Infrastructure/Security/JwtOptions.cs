namespace FairPlay.Sports.Infrastructure.Security;

/// <summary>
/// Bound from the <c>Jwt</c> configuration section. <see cref="SigningKey"/> is a secret:
/// it lives in user-secrets locally and in an environment variable / key vault in
/// production - never in a committed <c>appsettings.json</c>.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = null!;

    public string Audience { get; init; } = null!;

    public string SigningKey { get; init; } = null!;

    public int AccessTokenMinutes { get; init; } = 15;
}
