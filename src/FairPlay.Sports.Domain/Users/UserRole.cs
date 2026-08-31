namespace FairPlay.Sports.Domain.Users;

/// <summary>
/// The only two authorization levels the product has. There is no role-management
/// feature: every registration is a <see cref="Member"/>, and <see cref="Admin"/> is
/// granted out of band. It rides in the access token as the <c>role</c> claim.
/// </summary>
public enum UserRole
{
    Member = 0,
    Admin = 1
}
