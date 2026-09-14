namespace FairPlay.Sports.Api.Users;

public sealed record ChangeUserPasswordRequest(string CurrentPassword, string NewPassword);
