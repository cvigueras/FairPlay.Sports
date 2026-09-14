using FairPlay.Sports.Domain.Users;

namespace FairPlay.Sports.Domain.Tests.Users;

[TestFixture]
public class UserTests
{
    private static readonly DateTime Now = new(2026, 9, 8, 12, 0, 0, DateTimeKind.Utc);

    private static User Create(string email = "carlos@example.com") =>
        User.Create(Guid.NewGuid(), "carlos", email, "hash", teamId: null, Now);

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
