using FairPlay.Sports.Domain.Challenges;

namespace FairPlay.Sports.Domain.Tests.Challenges;

[TestFixture]
public class ChallengeTests
{
    private static readonly DateTime Now = new(2026, 9, 18, 12, 0, 0, DateTimeKind.Utc);

    private static Challenge Create(
        Guid? challengerTeamId = null, Guid? challengedTeamId = null, string? message = "Hola") =>
        Challenge.Create(
            Guid.NewGuid(),
            challengerTeamId ?? Guid.NewGuid(),
            challengedTeamId ?? Guid.NewGuid(),
            message,
            Now);

    [Test]
    public void Create_SetsStatusToPending()
    {
        Assert.That(Create().Status, Is.EqualTo(ChallengeStatus.Pending));
    }

    [Test]
    public void Create_TrimsTheMessage()
    {
        Assert.That(Create(message: "  Hola  ").Message, Is.EqualTo("Hola"));
    }

    [Test]
    public void Create_TurnsABlankMessageIntoNull()
    {
        Assert.That(Create(message: "   ").Message, Is.Null);
    }

    [Test]
    public void Create_RejectsAnEmptyId()
    {
        Assert.That(
            () => Challenge.Create(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), null, Now),
            Throws.ArgumentException);
    }

    [Test]
    public void Create_RejectsAnEmptyChallengerTeamId()
    {
        Assert.That(() => Create(challengerTeamId: Guid.Empty), Throws.ArgumentException);
    }

    [Test]
    public void Create_RejectsAnEmptyChallengedTeamId()
    {
        Assert.That(() => Create(challengedTeamId: Guid.Empty), Throws.ArgumentException);
    }

    [Test]
    public void Create_RejectsATeamChallengingItself()
    {
        var teamId = Guid.NewGuid();

        Assert.That(() => Create(challengerTeamId: teamId, challengedTeamId: teamId), Throws.ArgumentException);
    }

    [Test]
    public void Create_RejectsAMessageOverMaxLength()
    {
        Assert.That(
            () => Create(message: new string('x', Challenge.MaxMessageLength + 1)),
            Throws.ArgumentException);
    }

    [Test]
    public void Accept_SetsStatusAndRespondedAt()
    {
        var challenge = Create();
        var respondedAt = Now.AddHours(1);

        challenge.Accept(respondedAt);

        Assert.Multiple(() =>
        {
            Assert.That(challenge.Status, Is.EqualTo(ChallengeStatus.Accepted));
            Assert.That(challenge.RespondedAt, Is.EqualTo(respondedAt));
        });
    }

    [Test]
    public void Accept_WhenAlreadyResponded_Throws()
    {
        var challenge = Create();
        challenge.Accept(Now);

        Assert.That(() => challenge.Accept(Now), Throws.InvalidOperationException);
    }

    [Test]
    public void Reject_SetsStatusAndRespondedAt()
    {
        var challenge = Create();
        var respondedAt = Now.AddHours(1);

        challenge.Reject(respondedAt);

        Assert.Multiple(() =>
        {
            Assert.That(challenge.Status, Is.EqualTo(ChallengeStatus.Rejected));
            Assert.That(challenge.RespondedAt, Is.EqualTo(respondedAt));
        });
    }

    [Test]
    public void Reject_WhenAlreadyResponded_Throws()
    {
        var challenge = Create();
        challenge.Reject(Now);

        Assert.That(() => challenge.Reject(Now), Throws.InvalidOperationException);
    }
}
