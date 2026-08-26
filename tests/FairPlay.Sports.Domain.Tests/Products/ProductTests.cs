using FairPlay.Sports.Domain.Products;
using NUnit.Framework;

namespace FairPlay.Sports.Domain.Tests.Products;

[TestFixture]
public class ProductTests
{
    private static Product CreateValidProduct() =>
        new(Guid.NewGuid(), "Balón de fútbol", "Balón oficial talla 5", 29.99m, 10);

    [Test]
    public void Constructor_WithValidData_CreatesProduct()
    {
        var id = Guid.NewGuid();

        var product = new Product(id, "Balón de fútbol", "Balón oficial talla 5", 29.99m, 10);

        Assert.Multiple(() =>
        {
            Assert.That(product.Id, Is.EqualTo(id));
            Assert.That(product.Name, Is.EqualTo("Balón de fútbol"));
            Assert.That(product.Description, Is.EqualTo("Balón oficial talla 5"));
            Assert.That(product.Price, Is.EqualTo(29.99m));
            Assert.That(product.Stock, Is.EqualTo(10));
        });
    }

    [Test]
    public void Constructor_TrimsName()
    {
        var product = new Product(Guid.NewGuid(), "  Balón de fútbol  ", "desc", 1m, 1);

        Assert.That(product.Name, Is.EqualTo("Balón de fútbol"));
    }

    [Test]
    public void Constructor_WithNullDescription_DefaultsToEmptyString()
    {
        var product = new Product(Guid.NewGuid(), "Balón", null!, 1m, 1);

        Assert.That(product.Description, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Constructor_WithEmptyId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Product(Guid.Empty, "Balón", "desc", 1m, 1));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Constructor_WithInvalidName_ThrowsArgumentException(string? invalidName)
    {
        Assert.Throws<ArgumentException>(() =>
            new Product(Guid.NewGuid(), invalidName!, "desc", 1m, 1));
    }

    [Test]
    public void Constructor_WithNegativePrice_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Product(Guid.NewGuid(), "Balón", "desc", -1m, 1));
    }

    [Test]
    public void Constructor_WithNegativeStock_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Product(Guid.NewGuid(), "Balón", "desc", 1m, -1));
    }

    [Test]
    public void UpdateDetails_WithValidData_UpdatesAllMutableFields()
    {
        var product = CreateValidProduct();

        product.UpdateDetails("Nuevo nombre", "Nueva descripción", 49.99m, 5);

        Assert.Multiple(() =>
        {
            Assert.That(product.Name, Is.EqualTo("Nuevo nombre"));
            Assert.That(product.Description, Is.EqualTo("Nueva descripción"));
            Assert.That(product.Price, Is.EqualTo(49.99m));
            Assert.That(product.Stock, Is.EqualTo(5));
        });
    }

    [Test]
    public void UpdateDetails_PreservesOriginalId()
    {
        var product = CreateValidProduct();
        var originalId = product.Id;

        product.UpdateDetails("Nuevo nombre", "desc", 1m, 1);

        Assert.That(product.Id, Is.EqualTo(originalId));
    }

    [Test]
    public void UpdateDetails_WithInvalidName_ThrowsArgumentExceptionAndKeepsOriginalState()
    {
        var product = CreateValidProduct();

        Assert.Throws<ArgumentException>(() =>
            product.UpdateDetails("", "desc", 1m, 1));

        // Invariant: a failed update must not corrupt existing state.
        Assert.That(product.Name, Is.EqualTo("Balón de fútbol"));
    }
}
