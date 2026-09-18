namespace FairPlay.Sports.Domain.Challenges;

public sealed class Challenge
{
    public const int MaxMessageLength = 500;

    public Guid Id { get; }
    public Guid ChallengerTeamId { get; }
    public Guid ChallengedTeamId { get; }
    public string? Message { get; }
    public ChallengeStatus Status { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? RespondedAt { get; private set; }

    private Challenge(
        Guid id,
        Guid challengerTeamId,
        Guid challengedTeamId,
        string? message,
        DateTime createdAt)
    {
        Id = id;
        ChallengerTeamId = challengerTeamId;
        ChallengedTeamId = challengedTeamId;
        Message = message;
        Status = ChallengeStatus.Pending;
        CreatedAt = createdAt;
    }

    public static Challenge Create(
        Guid id,
        Guid challengerTeamId,
        Guid challengedTeamId,
        string? message,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Challenge id cannot be empty.", nameof(id));

        if (challengerTeamId == Guid.Empty)
            throw new ArgumentException("Challenger team id is required.", nameof(challengerTeamId));

        if (challengedTeamId == Guid.Empty)
            throw new ArgumentException("Challenged team id is required.", nameof(challengedTeamId));

        if (challengerTeamId == challengedTeamId)
            throw new ArgumentException("A team cannot challenge itself.", nameof(challengedTeamId));

        return new(id, challengerTeamId, challengedTeamId, ValidateMessage(message), createdAtUtc);
    }

    public void Accept(DateTime respondedAtUtc)
    {
        if (Status != ChallengeStatus.Pending)
            throw new InvalidOperationException("Only a pending challenge can be accepted.");

        Status = ChallengeStatus.Accepted;
        RespondedAt = respondedAtUtc;
    }

    public void Reject(DateTime respondedAtUtc)
    {
        if (Status != ChallengeStatus.Pending)
            throw new InvalidOperationException("Only a pending challenge can be rejected.");

        Status = ChallengeStatus.Rejected;
        RespondedAt = respondedAtUtc;
    }

    private static string? ValidateMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return null;

        var trimmed = message.Trim();
        if (trimmed.Length > MaxMessageLength)
            throw new ArgumentException($"Message cannot exceed {MaxMessageLength} characters.", nameof(message));

        return trimmed;
    }
}
