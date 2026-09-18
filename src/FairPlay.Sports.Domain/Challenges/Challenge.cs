namespace FairPlay.Sports.Domain.Challenges;

public sealed class Challenge
{
    public const int MaxMessageLength = 500;

    public Guid Id { get; }
    public Guid ChallengerTeamId { get; }
    public Guid ChallengedTeamId { get; }
    public Guid VenueTeamId { get; }
    public DateTime MatchDate { get; }
    public TeamKitSlot AwayKitSlot { get; }
    public string? Message { get; }
    public ChallengeStatus Status { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? RespondedAt { get; private set; }

    /// <summary>The team playing at home - whichever team's ground was chosen for the match.</summary>
    public Guid HomeTeamId => VenueTeamId;

    /// <summary>The team playing away - the other one.</summary>
    public Guid AwayTeamId => VenueTeamId == ChallengerTeamId ? ChallengedTeamId : ChallengerTeamId;

    private Challenge(
        Guid id,
        Guid challengerTeamId,
        Guid challengedTeamId,
        Guid venueTeamId,
        DateTime matchDate,
        TeamKitSlot awayKitSlot,
        string? message,
        DateTime createdAt)
    {
        Id = id;
        ChallengerTeamId = challengerTeamId;
        ChallengedTeamId = challengedTeamId;
        VenueTeamId = venueTeamId;
        MatchDate = matchDate;
        AwayKitSlot = awayKitSlot;
        Message = message;
        Status = ChallengeStatus.Pending;
        CreatedAt = createdAt;
    }

    public static Challenge Create(
        Guid id,
        Guid challengerTeamId,
        Guid challengedTeamId,
        Guid venueTeamId,
        DateTime matchDate,
        TeamKitSlot awayKitSlot,
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

        if (venueTeamId != challengerTeamId && venueTeamId != challengedTeamId)
        {
            throw new ArgumentException(
                "The venue must belong to the challenger or the challenged team.", nameof(venueTeamId));
        }

        if (!Enum.IsDefined(awayKitSlot))
            throw new ArgumentException($"'{awayKitSlot}' is not a valid {nameof(TeamKitSlot)}.", nameof(awayKitSlot));

        return new(
            id, challengerTeamId, challengedTeamId, venueTeamId, matchDate, awayKitSlot,
            ValidateMessage(message), createdAtUtc);
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
