namespace FairPlay.Sports.Application.Auth;

/// <summary>A signed JWT plus the moment it stops being valid.</summary>
public sealed record AccessToken(string Value, DateTime ExpiresAtUtc);
