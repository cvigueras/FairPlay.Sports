using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Domain.Tests.Teams;

[TestFixture]
public class KitColorsTests
{
    [Test]
    public void Ctor_TrimsBothColours()
    {
        var colors = new KitColors("  Blue  ", "  White  ");

        Assert.Multiple(() =>
        {
            Assert.That(colors.Primary, Is.EqualTo("Blue"));
            Assert.That(colors.Secondary, Is.EqualTo("White"));
        });
    }

    [TestCase("", "White")]
    [TestCase("   ", "White")]
    [TestCase("Blue", "")]
    [TestCase("Blue", "   ")]
    public void Ctor_WithBlankColour_Throws(string primary, string secondary) =>
        Assert.That(() => new KitColors(primary, secondary), Throws.ArgumentException);

    [Test]
    public void Ctor_WithOverlongColour_Throws() =>
        Assert.That(
            () => new KitColors(new string('x', KitColors.MaxColourLength + 1), "White"),
            Throws.ArgumentException);

    [Test]
    public void Equality_IsByValue()
    {
        var a = new KitColors("Blue", "White");
        var b = new KitColors("Blue", "White");
        var c = new KitColors("Blue", "Red");

        Assert.Multiple(() =>
        {
            Assert.That(a, Is.EqualTo(b));
            Assert.That(a, Is.Not.EqualTo(c));
        });
    }
}
