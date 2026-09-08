namespace FairPlay.Sports.Api.Users;

public sealed record RegisterUserRequest(string UserName, string Email, string Password, Guid? TeamId = null);
