using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Domain.Tests.Teams;

[TestFixture]
public class TeamMemberTests
{
    private static readonly DateTime Now = new(2026, 9, 16, 12, 0, 0, DateTimeKind.Utc);

    private static TeamMember Create(
        Guid? teamId = null,
        Guid? userId = null,
        TeamMemberRole role = TeamMemberRole.Player,
        string displayName = "Carlos V.") =>
        TeamMember.Create(Guid.NewGuid(), teamId ?? Guid.NewGuid(), userId ?? Guid.NewGuid(), role, displayName, Now);

    [Test]
    public void Create_TrimsTheDisplayName()
    {
        var member = Create(displayName: "  Carlos V.  ");

        Assert.That(member.DisplayName, Is.EqualTo("Carlos V."));
    }

    [Test]
    public void Create_RejectsAnEmptyId()
    {
        Assert.That(
            () => TeamMember.Create(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), TeamMemberRole.Player, "Carlos V.", Now),
            Throws.ArgumentException);
    }

    [Test]
    public void Create_RejectsAnEmptyTeamId()
    {
        Assert.That(() => Create(teamId: Guid.Empty), Throws.ArgumentException);
    }

    [Test]
    public void Create_RejectsAnEmptyUserId()
    {
        Assert.That(() => Create(userId: Guid.Empty), Throws.ArgumentException);
    }

    [Test]
    public void Create_RejectsABlankDisplayName()
    {
        Assert.That(() => Create(displayName: "   "), Throws.ArgumentException);
    }

    [Test]
    public void Create_RejectsADisplayNameOverMaxLength()
    {
        Assert.That(() => Create(displayName: new string('x', TeamMember.MaxDisplayNameLength + 1)), Throws.ArgumentException);
    }

    [Test]
    public void Create_RejectsAnUndefinedRole()
    {
        Assert.That(() => Create(role: (TeamMemberRole)999), Throws.ArgumentException);
    }
}
