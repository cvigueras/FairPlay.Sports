using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.Domain.Users;

namespace FairPlay.Sports.Domain.Tests.Users;

[TestFixture]
public class UserTests
{
    private static readonly DateTime Now = new(2026, 9, 8, 12, 0, 0, DateTimeKind.Utc);

    private static User Create(
        string email = "carlos@example.com",
        string firstName = "Carlos",
        string lastName = "Vigueras",
        TeamMemberRole primaryRole = TeamMemberRole.Player,
        string privacyPolicyVersion = "2026-09-30") =>
        User.Create(Guid.NewGuid(), "carlos", email, "hash", Now, firstName, lastName, primaryRole, privacyPolicyVersion);

    [Test]
    public void Create_TrimsTheNames_AndKeepsThePrimaryRole()
    {
        var user = Create(firstName: "  Carlos ", lastName: " Vigueras  ", primaryRole: TeamMemberRole.Coach);

        Assert.Multiple(() =>
        {
            Assert.That(user.FirstName, Is.EqualTo("Carlos"));
            Assert.That(user.LastName, Is.EqualTo("Vigueras"));
            Assert.That(user.PrimaryRole, Is.EqualTo(TeamMemberRole.Coach));
        });
    }

    [Test]
    public void Create_RecordsThePrivacyPolicyAcceptance_AtRegistrationTime()
    {
        var user = Create(privacyPolicyVersion: "2026-09-30");

        Assert.Multiple(() =>
        {
            Assert.That(user.PrivacyPolicyVersion, Is.EqualTo("2026-09-30"));
            Assert.That(user.PrivacyAcceptedAt, Is.EqualTo(Now));
        });
    }

    [TestCase("")]
    [TestCase("  ")]
    public void Create_RejectsABlankFirstName(string firstName)
    {
        Assert.That(() => Create(firstName: firstName), Throws.ArgumentException);
    }

    [TestCase("")]
    [TestCase("  ")]
    public void Create_RejectsABlankLastName(string lastName)
    {
        Assert.That(() => Create(lastName: lastName), Throws.ArgumentException);
    }

    [Test]
    public void Create_RejectsAnUnknownPrimaryRole()
    {
        Assert.That(() => Create(primaryRole: (TeamMemberRole)999), Throws.ArgumentException);
    }

    [Test]
    public void Create_RejectsABlankPrivacyPolicyVersion()
    {
        Assert.That(() => Create(privacyPolicyVersion: " "), Throws.ArgumentException);
    }

    [Test]
    public void Create_NormalisesTheEmail_ToTrimmedLowercase()
    {
        var user = Create("  Carlos@Example.COM  ");

        Assert.That(user.Email, Is.EqualTo("carlos@example.com"));
    }

    [Test]
    public void Create_RejectsAnEmailWithoutAnAtSign()
    {
        Assert.That(() => Create("not-an-email"), Throws.ArgumentException);
    }

    [Test]
    public void UpdateProfile_TrimsTheUserName_AndNormalisesTheEmail()
    {
        var user = Create();

        user.UpdateProfile("  new-name  ", "  New@Example.COM  ");

        Assert.Multiple(() =>
        {
            Assert.That(user.UserName, Is.EqualTo("new-name"));
            Assert.That(user.Email, Is.EqualTo("new@example.com"));
        });
    }

    [Test]
    public void UpdateProfile_RejectsAnEmailWithoutAnAtSign()
    {
        var user = Create();

        Assert.That(() => user.UpdateProfile("new-name", "not-an-email"), Throws.ArgumentException);
    }

    [Test]
    public void ChangePasswordHash_ReplacesTheStoredHash()
    {
        var user = Create();

        user.ChangePasswordHash("a-new-hash");

        Assert.That(user.PasswordHash, Is.EqualTo("a-new-hash"));
    }
}
