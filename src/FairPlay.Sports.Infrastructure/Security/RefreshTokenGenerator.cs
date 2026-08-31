using System.Security.Cryptography;
using System.Text;
using FairPlay.Sports.Application.Auth;

namespace FairPlay.Sports.Infrastructure.Security;

/// <summary>
/// Driven adapter: 256 bits from the OS CSPRNG as the raw token, SHA-256 for the stored
/// hash. The token has no structure, so a plain hash (no salt/KDF) is enough to make a
/// database leak useless while keeping lookups a single indexed equality check.
/// </summary>
internal sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    private const int TokenSizeInBytes = 32;

    public string NewToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(TokenSizeInBytes));

    public string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
