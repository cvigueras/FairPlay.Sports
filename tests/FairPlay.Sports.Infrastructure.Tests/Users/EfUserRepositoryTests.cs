using FairPlay.Sports.Domain.Users;
using FairPlay.Sports.Infrastructure.Persistence;
using FairPlay.Sports.Infrastructure.Users;
using FairPlay.Sports.TestSupport.Users;
using Microsoft.EntityFrameworkCore;

namespace FairPlay.Sports.Infrastructure.Tests.Users;

/// <summary>
/// Integration tests for <see cref="EfUserRepository"/> against a real SQL Server (Testcontainers).
/// They cover what an in-memory provider cannot: the unique indexes on Email/UserName, the column
/// length limits, the server-side <c>ORDER BY</c>, and that writes only land once the unit of work
/// commits. Test data comes from <see cref="UserMother"/>.
/// </summary>
[TestFixture]
public class EfUserRepositoryTests : RepositoryTestBase
{
    private static async Task SeedAsync(params User[] users)
    {
        await using var context = NewContext();
        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }

    [Test]
    public async Task AddAsync_thenCommit_persistsTheUser()
    {
        var user = UserMother.DomainUser();

        await using (var arrange = NewContext())
        {
            var repository = new EfUserRepository(arrange);
            var unitOfWork = new UnitOfWork(arrange);
            await repository.AddAsync(user);
            await unitOfWork.SaveChangesAsync();
        }

        await using var assert = NewContext();
        var persisted = await assert.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == user.Id);
        Assert.That(persisted, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(persisted!.UserName, Is.EqualTo(user.UserName));
            Assert.That(persisted.Email, Is.EqualTo(user.Email));
            Assert.That(persisted.Team, Is.EqualTo(user.Team));
            Assert.That(persisted.PasswordHash, Is.EqualTo(user.PasswordHash));
            Assert.That(persisted.Active, Is.True);
            Assert.That(persisted.CreatedAt, Is.EqualTo(user.CreatedAt).Within(TimeSpan.FromMilliseconds(10)));
        });
    }

    [Test]
    public async Task AddAsync_withoutCommit_doesNotPersist()
    {
        var user = UserMother.DomainUser();

        await using (var arrange = NewContext())
        {
            var repository = new EfUserRepository(arrange);
            await repository.AddAsync(user);
            // no unit-of-work commit
        }

        await using var assert = NewContext();
        Assert.That(await assert.Users.AnyAsync(u => u.Id == user.Id), Is.False);
    }

    [Test]
    public async Task Commit_withDuplicateEmail_throwsDbUpdateException()
    {
        await SeedAsync(UserMother.DomainUser(email: "dupe@example.com", userName: "first"));

        await using var context = NewContext();
        var repository = new EfUserRepository(context);
        var unitOfWork = new UnitOfWork(context);
        await repository.AddAsync(UserMother.DomainUser(email: "dupe@example.com", userName: "second"));

        Assert.That(async () => await unitOfWork.SaveChangesAsync(), Throws.InstanceOf<DbUpdateException>());
    }

    [Test]
    public async Task Commit_withDuplicateUserName_throwsDbUpdateException()
    {
        await SeedAsync(UserMother.DomainUser(userName: "dupe", email: "a@example.com"));

        await using var context = NewContext();
        var repository = new EfUserRepository(context);
        var unitOfWork = new UnitOfWork(context);
        await repository.AddAsync(UserMother.DomainUser(userName: "dupe", email: "b@example.com"));

        Assert.That(async () => await unitOfWork.SaveChangesAsync(), Throws.InstanceOf<DbUpdateException>());
    }

    [Test]
    public async Task Commit_withUserNameOverMaxLength_throwsDbUpdateException()
    {
        await using var context = NewContext();
        context.Users.Add(UserMother.DomainUser(userName: new string('x', 51)));

        Assert.That(async () => await context.SaveChangesAsync(), Throws.InstanceOf<DbUpdateException>());
    }

    [Test]
    public async Task GetByIdAsync_whenPresent_returnsTheUser()
    {
        var user = UserMother.DomainUser();
        await SeedAsync(user);

        await using var context = NewContext();
        var found = await new EfUserRepository(context).GetByIdAsync(user.Id);

        Assert.That(found, Is.Not.Null);
        Assert.That(found!.Id, Is.EqualTo(user.Id));
    }

    [Test]
    public async Task GetByIdAsync_whenMissing_returnsNull()
    {
        await using var context = NewContext();

        Assert.That(await new EfUserRepository(context).GetByIdAsync(Guid.NewGuid()), Is.Null);
    }

    [Test]
    public async Task GetByIdAsync_doesNotTrackTheEntity()
    {
        var user = UserMother.DomainUser();
        await SeedAsync(user);

        await using var context = NewContext();
        await new EfUserRepository(context).GetByIdAsync(user.Id);

        Assert.That(context.ChangeTracker.Entries<User>(), Is.Empty);
    }

    [Test]
    public async Task GetAllAsync_ordersByUserNameAscending()
    {
        await SeedAsync(
            UserMother.DomainUser(userName: "charlie", email: "c@example.com"),
            UserMother.DomainUser(userName: "alice", email: "a@example.com"),
            UserMother.DomainUser(userName: "bob", email: "b@example.com"));

        await using var context = NewContext();
        var all = await new EfUserRepository(context).GetAllAsync();

        Assert.That(all.Select(u => u.UserName), Is.EqualTo(new[] { "alice", "bob", "charlie" }));
    }

    [Test]
    public async Task GetAllAsync_whenEmpty_returnsEmptyList()
    {
        await using var context = NewContext();

        Assert.That(await new EfUserRepository(context).GetAllAsync(), Is.Empty);
    }

    [Test]
    public async Task ExistsByEmailAsync_reflectsWhetherRowExists()
    {
        await SeedAsync(UserMother.DomainUser(email: "known@example.com"));

        await using var context = NewContext();
        var repository = new EfUserRepository(context);

        Assert.That(await repository.ExistsByEmailAsync("known@example.com"), Is.True);
        Assert.That(await repository.ExistsByEmailAsync("unknown@example.com"), Is.False);
    }

    [Test]
    public async Task ExistsByUserNameAsync_reflectsWhetherRowExists()
    {
        await SeedAsync(UserMother.DomainUser(userName: "known"));

        await using var context = NewContext();
        var repository = new EfUserRepository(context);

        Assert.That(await repository.ExistsByUserNameAsync("known"), Is.True);
        Assert.That(await repository.ExistsByUserNameAsync("unknown"), Is.False);
    }
}
