using System.Security.Claims;

namespace FairPlay.Sports.Api.Common;

public static class ClaimsPrincipalExtensions
{
    /// <summary>The authenticated user's id, from the JWT's "sub" claim.</summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var sub = user.FindFirst("sub")?.Value
            ?? throw new InvalidOperationException("The current user has no 'sub' claim.");

        return Guid.Parse(sub);
    }
}
