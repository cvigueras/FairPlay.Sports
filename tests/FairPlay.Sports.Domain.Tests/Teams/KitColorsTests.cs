using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Domain.Tests.Teams;

[TestFixture]
public class KitColorsTests
{
    [Test]
    public void Ctor_TrimsBothColours()
    {
        var colors = new KitColors("  Blue  ", "  White  ", KitPattern.Plain);

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
        Assert.That(() => new KitColors(primary, secondary, KitPattern.Plain), Throws.ArgumentException);

    [Test]
    public void Ctor_WithOverlongColour_Throws() =>
        Assert.That(
            () => new KitColors(new string('x', KitColors.MaxColourLength + 1), "White", KitPattern.Plain),
            Throws.ArgumentException);

    [Test]
    public void Ctor_WithDefaultPattern_Throws() =>
        Assert.That(() => new KitColors("Blue", "White", KitPattern.Default), Throws.ArgumentException);

    [Test]
    public void Ctor_WithUndefinedPattern_Throws() =>
        Assert.That(() => new KitColors("Blue", "White", (KitPattern)999), Throws.ArgumentException);

    [Test]
    public void Equality_IsByValue()
    {
        var a = new KitColors("Blue", "White", KitPattern.Stripes);
        var b = new KitColors("Blue", "White", KitPattern.Stripes);
        var c = new KitColors("Blue", "Red", KitPattern.Stripes);
        var d = new KitColors("Blue", "White", KitPattern.Hoops);

        Assert.Multiple(() =>
        {
            Assert.That(a, Is.EqualTo(b));
            Assert.That(a, Is.Not.EqualTo(c));
            Assert.That(a, Is.Not.EqualTo(d));
        });
    }
}
