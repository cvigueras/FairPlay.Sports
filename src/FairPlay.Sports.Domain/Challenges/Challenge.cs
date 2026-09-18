namespace FairPlay.Sports.Domain.Challenges;

public sealed class Challenge
{
    public const int MaxMessageLength = 500;

    public Guid Id { get; }
    public Guid ChallengerTeamId { get; }
    public Guid ChallengedTeamId { get; }
    public Guid VenueTeamId { get; }
    public DateTime MatchDate { get; }

    /// <summary>
    /// Which of the home team's kits it wears - its first by default, but the challenger may
    /// pick its second instead when it's the one playing at home (the challenged team's kit is
    /// never chosen this way). Null only when the home team has no kit configured at all.
    /// </summary>
    public TeamKitSlot? HomeKitSlot { get; }

    /// <summary>
    /// Which of the away team's kits avoids clashing with the home team's (whichever
    /// <see cref="HomeKitSlot"/> resolved to), chosen automatically - the away team never picks
    /// this itself. Null when it has none to fall back on - no kit configured at all, or none of
    /// its kits avoid the clash - a challenge is still allowed to go ahead either way.
    /// </summary>
    public TeamKitSlot? AwayKitSlot { get; }

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
        TeamKitSlot? homeKitSlot,
        TeamKitSlot? awayKitSlot,
        string? message,
        DateTime createdAt)
    {
        Id = id;
        ChallengerTeamId = challengerTeamId;
        ChallengedTeamId = challengedTeamId;
        VenueTeamId = venueTeamId;
        MatchDate = matchDate;
        HomeKitSlot = homeKitSlot;
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
        TeamKitSlot? homeKitSlot,
        TeamKitSlot? awayKitSlot,
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

        if (homeKitSlot is not null && !Enum.IsDefined(homeKitSlot.Value))
            throw new ArgumentException($"'{homeKitSlot}' is not a valid {nameof(TeamKitSlot)}.", nameof(homeKitSlot));

        if (awayKitSlot is not null && !Enum.IsDefined(awayKitSlot.Value))
            throw new ArgumentException($"'{awayKitSlot}' is not a valid {nameof(TeamKitSlot)}.", nameof(awayKitSlot));

        return new(
            id, challengerTeamId, challengedTeamId, venueTeamId, matchDate, homeKitSlot, awayKitSlot,
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
