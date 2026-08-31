using FairPlay.Sports.Domain.Auth;
using FairPlay.Sports.Domain.Users;
using FairPlay.Sports.Infrastructure.Auth;
using FairPlay.Sports.Infrastructure.Persistence;
using FairPlay.Sports.TestSupport.Auth;
using FairPlay.Sports.TestSupport.Users;
using Microsoft.EntityFrameworkCore;

namespace FairPlay.Sports.Infrastructure.Tests.Auth;

/// <summary>
/// Integration tests for <see cref="EfRefreshTokenRepository"/> against a real SQL Server
/// (Testcontainers). They cover what an in-memory provider cannot: the unique index on the
/// token hash, the <c>UserId</c> foreign key, the server-side "active and not expired"
/// filter, and that <see cref="EfRefreshTokenRepository.GetByTokenHashAsync"/> returns a
/// tracked aggregate whose <c>Revoke</c> is persisted on commit.
/// </summary>
[TestFixture]
public class EfRefreshTokenRepositoryTests : RepositoryTestBase
{
    private static readonly DateTime Now = new(2026, 8, 29, 12, 0, 0, DateTimeKind.Utc);

    private static async Task<User> SeedUserAsync(Guid? id = null)
    {
        var user = UserMother.DomainUser(id: id ?? Guid.NewGuid());
        await using var context = NewContext();
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private static async Task SeedAsync(params RefreshToken[] tokens)
    {
        await using var context = NewContext();
        context.RefreshTokens.AddRange(tokens);
        await context.SaveChangesAsync();
    }

    [Test]
    public async Task AddAsync_thenCommit_persistsTheToken()
    {
        var user = await SeedUserAsync();
        var token = AuthMother.DomainRefreshToken(userId: user.Id, tokenHash: "hash-1");

        await using (var arrange = NewContext())
        {
            var repository = new EfRefreshTokenRepository(arrange);
            var unitOfWork = new UnitOfWork(arrange);
            await repository.AddAsync(token);
            await unitOfWork.SaveChangesAsync();
        }

        await using var assert = NewContext();
        var persisted = await assert.RefreshTokens.AsNoTracking().SingleOrDefaultAsync(t => t.Id == token.Id);
        Assert.That(persisted, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(persisted!.UserId, Is.EqualTo(user.Id));
            Assert.That(persisted.TokenHash, Is.EqualTo("hash-1"));
            Assert.That(persisted.RevokedAtUtc, Is.Null);
        });
    }

    [Test]
    public async Task AddAsync_withUnknownUserId_violatesForeignKey()
    {
        await using var context = NewContext();
        var repository = new EfRefreshTokenRepository(context);
        var unitOfWork = new UnitOfWork(context);
        await repository.AddAsync(AuthMother.DomainRefreshToken(userId: Guid.NewGuid(), tokenHash: "orphan"));

        Assert.That(async () => await unitOfWork.SaveChangesAsync(), Throws.InstanceOf<DbUpdateException>());
    }

    [Test]
    public async Task Commit_withDuplicateTokenHash_throwsDbUpdateException()
    {
        var user = await SeedUserAsync();
        await SeedAsync(AuthMother.DomainRefreshToken(userId: user.Id, tokenHash: "dupe"));

        await using var context = NewContext();
        var repository = new EfRefreshTokenRepository(context);
        var unitOfWork = new UnitOfWork(context);
        await repository.AddAsync(AuthMother.DomainRefreshToken(userId: user.Id, tokenHash: "dupe"));

        Assert.That(async () => await unitOfWork.SaveChangesAsync(), Throws.InstanceOf<DbUpdateException>());
    }

    [Test]
    public async Task GetByTokenHashAsync_returnsTrackedToken_soRevokeIsPersistedOnCommit()
    {
        var user = await SeedUserAsync();
        var token = AuthMother.DomainRefreshToken(userId: user.Id, tokenHash: "to-revoke");
        await SeedAsync(token);

        await using (var act = NewContext())
        {
            var repository = new EfRefreshTokenRepository(act);
            var unitOfWork = new UnitOfWork(act);
            var loaded = await repository.GetByTokenHashAsync("to-revoke");
            loaded!.Revoke(Now);
            await unitOfWork.SaveChangesAsync();
        }

        await using var assert = NewContext();
        var persisted = await assert.RefreshTokens.AsNoTracking().SingleAsync(t => t.Id == token.Id);
        Assert.That(persisted.RevokedAtUtc, Is.EqualTo(Now).Within(TimeSpan.FromMilliseconds(10)));
    }

    [Test]
    public async Task GetByTokenHashAsync_whenMissing_returnsNull()
    {
        await using var context = NewContext();

        Assert.That(await new EfRefreshTokenRepository(context).GetByTokenHashAsync("nope"), Is.Null);
    }

    [Test]
    public async Task GetActiveByUserIdAsync_returnsOnlyActiveNonExpiredTokensForThatUser()
    {
        var user = await SeedUserAsync();
        var other = await SeedUserAsync();

        var active = AuthMother.DomainRefreshToken(userId: user.Id, tokenHash: "active", createdAtUtc: Now.AddDays(-1), expiresAtUtc: Now.AddDays(13));
        var expired = AuthMother.DomainRefreshToken(userId: user.Id, tokenHash: "expired", createdAtUtc: Now.AddDays(-30), expiresAtUtc: Now.AddDays(-1));
        var revoked = AuthMother.DomainRefreshToken(userId: user.Id, tokenHash: "revoked");
        revoked.Revoke(Now.AddDays(-2));
        var otherUsers = AuthMother.DomainRefreshToken(userId: other.Id, tokenHash: "other-user");
        await SeedAsync(active, expired, revoked, otherUsers);

        await using var context = NewContext();
        var result = await new EfRefreshTokenRepository(context).GetActiveByUserIdAsync(user.Id, Now);

        Assert.That(result.Select(t => t.TokenHash), Is.EquivalentTo(new[] { "active" }));
    }
}
