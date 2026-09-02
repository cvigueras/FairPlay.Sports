namespace FairPlay.Sports.Application.Teams;

/// <summary>A team crest image ready to be streamed back to a client.</summary>
public sealed record TeamCrest(byte[] Content, string ContentType);
