using FairPlay.Sports.Domain.Products;
using FairPlay.Sports.Infrastructure.Products;
using NUnit.Framework;

namespace FairPlay.Sports.Infrastructure.Tests.Products;

[TestFixture]
public class InMemoryProductRepositoryTests
{
    private InMemoryProductRepository _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new InMemoryProductRepository();
    }

    [Test]
    public async Task GetAllAsync_OnFreshRepository_ReturnsSeedDummyData()
    {
        var products = await _sut.GetAllAsync();

        Assert.That(products, Has.Count.EqualTo(5));
    }

    [Test]
    public async Task GetByIdAsync_WithSeededId_ReturnsThatProduct()
    {
        var seeded = (await _sut.GetAllAsync()).First();

        var found = await _sut.GetByIdAsync(seeded.Id);

        Assert.That(found, Is.Not.Null);
        Assert.That(found!.Id, Is.EqualTo(seeded.Id));
    }

    [Test]
    public async Task GetByIdAsync_WithUnknownId_ReturnsNull()
    {
        var found = await _sut.GetByIdAsync(Guid.NewGuid());

        Assert.That(found, Is.Null);
    }

    [Test]
    public async Task AddAsync_AddsNewProductThatBecomesRetrievable()
    {
        var product = new Product(Guid.NewGuid(), "Test product", "desc", 9.99m, 3);

        await _sut.AddAsync(product);
        var found = await _sut.GetByIdAsync(product.Id);
        var all = await _sut.GetAllAsync();

        Assert.That(found, Is.Not.Null);
        Assert.That(all, Has.Count.EqualTo(6));
    }

    [Test]
    public async Task UpdateAsync_WhenProductExists_ReplacesItAndReturnsTrue()
    {
        var seeded = (await _sut.GetAllAsync()).First();
        seeded.UpdateDetails("Updated name", "Updated desc", 1m, 1);

        var updated = await _sut.UpdateAsync(seeded);
        var found = await _sut.GetByIdAsync(seeded.Id);

        Assert.That(updated, Is.True);
        Assert.That(found!.Name, Is.EqualTo("Updated name"));
    }

    [Test]
    public async Task UpdateAsync_WhenProductDoesNotExist_ReturnsFalse()
    {
        var nonExisting = new Product(Guid.NewGuid(), "Ghost", "desc", 1m, 1);

        var updated = await _sut.UpdateAsync(nonExisting);

        Assert.That(updated, Is.False);
    }

    [Test]
    public async Task DeleteAsync_WhenProductExists_RemovesItAndReturnsTrue()
    {
        var seeded = (await _sut.GetAllAsync()).First();

        var deleted = await _sut.DeleteAsync(seeded.Id);
        var found = await _sut.GetByIdAsync(seeded.Id);

        Assert.That(deleted, Is.True);
        Assert.That(found, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_WhenProductDoesNotExist_ReturnsFalse()
    {
        var deleted = await _sut.DeleteAsync(Guid.NewGuid());

        Assert.That(deleted, Is.False);
    }
}
