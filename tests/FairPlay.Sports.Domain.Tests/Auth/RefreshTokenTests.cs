using FairPlay.Sports.Domain.Auth;

namespace FairPlay.Sports.Domain.Tests.Auth;

[TestFixture]
public class RefreshTokenTests
{
    private static readonly DateTime Now = new(2026, 8, 29, 12, 0, 0, DateTimeKind.Utc);

    private static RefreshToken Issue(DateTime? created = null, DateTime? expires = null) =>
        RefreshToken.Issue(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "token-hash",
            created ?? Now,
            expires ?? Now.AddDays(14));

    [Test]
    public void Issue_TrimsTheHash_AndStartsActive()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), "  hash  ", Now, Now.AddDays(1));

        Assert.Multiple(() =>
        {
            Assert.That(token.TokenHash, Is.EqualTo("hash"));
            Assert.That(token.RevokedAtUtc, Is.Null);
            Assert.That(token.IsActive(Now), Is.True);
        });
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Issue_WithBlankHash_Throws(string hash) =>
        Assert.That(
            () => RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), hash, Now, Now.AddDays(1)),
            Throws.ArgumentException);

    [Test]
    public void Issue_WithEmptyUserId_Throws() =>
        Assert.That(
            () => RefreshToken.Issue(Guid.NewGuid(), Guid.Empty, "hash", Now, Now.AddDays(1)),
            Throws.ArgumentException);

    [Test]
    public void Issue_WhenExpiryNotAfterCreation_Throws() =>
        Assert.That(
            () => RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), "hash", Now, Now),
            Throws.ArgumentException);

    [Test]
    public void IsActive_IsFalse_OnceExpired()
    {
        var token = Issue(expires: Now.AddHours(1));

        Assert.That(token.IsActive(Now.AddHours(2)), Is.False);
    }

    [Test]
    public void Revoke_MarksTheTokenSpent_AndLinksTheReplacement()
    {
        var token = Issue();
        var replacement = Guid.NewGuid();

        token.Revoke(Now.AddMinutes(5), replacement);

        Assert.Multiple(() =>
        {
            Assert.That(token.RevokedAtUtc, Is.EqualTo(Now.AddMinutes(5)));
            Assert.That(token.ReplacedByTokenId, Is.EqualTo(replacement));
            Assert.That(token.IsActive(Now.AddMinutes(6)), Is.False);
        });
    }

    [Test]
    public void Revoke_IsIdempotent()
    {
        var token = Issue();
        token.Revoke(Now.AddMinutes(5));

        token.Revoke(Now.AddMinutes(10), Guid.NewGuid());

        Assert.Multiple(() =>
        {
            Assert.That(token.RevokedAtUtc, Is.EqualTo(Now.AddMinutes(5)));
            Assert.That(token.ReplacedByTokenId, Is.Null);
        });
    }
}
