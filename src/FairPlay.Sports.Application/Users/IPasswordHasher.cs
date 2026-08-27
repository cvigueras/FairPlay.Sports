namespace FairPlay.Sports.Application.Users;

/// <summary>
/// Driven port for turning a clear-text password into a storable hash and verifying
/// it later. Implemented in Infrastructure over ASP.NET Core's password hasher.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string passwordHash, string providedPassword);
}
