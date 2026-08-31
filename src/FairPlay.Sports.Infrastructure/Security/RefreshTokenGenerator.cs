using System.Security.Cryptography;
using System.Text;
using FairPlay.Sports.Application.Auth;

namespace FairPlay.Sports.Infrastructure.Security;


internal sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    private const int TokenSizeInBytes = 32;

    public string NewToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(TokenSizeInBytes));

    public string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
