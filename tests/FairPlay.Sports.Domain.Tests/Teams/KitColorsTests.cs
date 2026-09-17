using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Domain.Tests.Teams;

[TestFixture]
public class KitColorsTests
{
    [Test]
    public void Ctor_TrimsAllColours()
    {
        var colors = new KitColors("  Blue  ", "  White  ", "  Navy  ", KitPattern.Plain);

        Assert.Multiple(() =>
        {
            Assert.That(colors.Primary, Is.EqualTo("Blue"));
            Assert.That(colors.Secondary, Is.EqualTo("White"));
            Assert.That(colors.ShortsColor, Is.EqualTo("Navy"));
        });
    }

    [TestCase("", "White", "Navy")]
    [TestCase("   ", "White", "Navy")]
    [TestCase("Blue", "", "Navy")]
    [TestCase("Blue", "   ", "Navy")]
    [TestCase("Blue", "White", "")]
    [TestCase("Blue", "White", "   ")]
    public void Ctor_WithBlankColour_Throws(string primary, string secondary, string shortsColor) =>
        Assert.That(() => new KitColors(primary, secondary, shortsColor, KitPattern.Plain), Throws.ArgumentException);

    [Test]
    public void Ctor_WithOverlongColour_Throws() =>
        Assert.That(
            () => new KitColors(new string('x', KitColors.MaxColourLength + 1), "White", "Navy", KitPattern.Plain),
            Throws.ArgumentException);

    [Test]
    public void Ctor_WithDefaultPattern_Throws() =>
        Assert.That(() => new KitColors("Blue", "White", "Navy", KitPattern.Default), Throws.ArgumentException);

    [Test]
    public void Ctor_WithUndefinedPattern_Throws() =>
        Assert.That(() => new KitColors("Blue", "White", "Navy", (KitPattern)999), Throws.ArgumentException);

    [Test]
    public void Equality_IsByValue()
    {
        var a = new KitColors("Blue", "White", "Navy", KitPattern.Stripes);
        var b = new KitColors("Blue", "White", "Navy", KitPattern.Stripes);
        var c = new KitColors("Blue", "Red", "Navy", KitPattern.Stripes);
        var d = new KitColors("Blue", "White", "Navy", KitPattern.Hoops);
        var e = new KitColors("Blue", "White", "Black", KitPattern.Stripes);

        Assert.Multiple(() =>
        {
            Assert.That(a, Is.EqualTo(b));
            Assert.That(a, Is.Not.EqualTo(c));
            Assert.That(a, Is.Not.EqualTo(d));
            Assert.That(a, Is.Not.EqualTo(e));
        });
    }
}
