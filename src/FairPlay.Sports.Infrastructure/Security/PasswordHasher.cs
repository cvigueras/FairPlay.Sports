using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Domain.Users;
using Microsoft.AspNetCore.Identity;
using IdentityPasswordHasher = Microsoft.AspNetCore.Identity.PasswordHasher<FairPlay.Sports.Domain.Users.User>;

namespace FairPlay.Sports.Infrastructure.Security;

/// <summary>
/// Driven adapter over ASP.NET Core's PBKDF2 password hasher. The hasher does not use
/// the user argument, so a placeholder instance is passed.
/// </summary>
internal sealed class PasswordHasher : IPasswordHasher
{
    private static readonly User HashingContext = User.Create(
        Guid.NewGuid(), "hashing-context", "hashing@context.local", "placeholder", "none", DateTime.UnixEpoch);

    private readonly IdentityPasswordHasher _inner = new();

    public string Hash(string password) => _inner.HashPassword(HashingContext, password);

    public bool Verify(string passwordHash, string providedPassword) =>
        _inner.VerifyHashedPassword(HashingContext, passwordHash, providedPassword)
            is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
}
