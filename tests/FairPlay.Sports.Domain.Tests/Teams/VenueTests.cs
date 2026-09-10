using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Domain.Tests.Teams;

[TestFixture]
public class VenueTests
{
    [Test]
    public void Ctor_TrimsNameAndAddress_AndKeepsSurfaceAndMapsUrl()
    {
        var venue = new Venue("  Pabellón  ", "  Calle 1  ", PitchSurface.Indoor, "https://maps.google.com/?q=x");

        Assert.Multiple(() =>
        {
            Assert.That(venue.Name, Is.EqualTo("Pabellón"));
            Assert.That(venue.Address, Is.EqualTo("Calle 1"));
            Assert.That(venue.Surface, Is.EqualTo(PitchSurface.Indoor));
            Assert.That(venue.MapsUrl, Is.EqualTo("https://maps.google.com/?q=x"));
        });
    }

    [Test]
    public void Ctor_WithoutMapsUrl_LeavesItNull() =>
        Assert.That(new Venue("V", "A", PitchSurface.NaturalGrass).MapsUrl, Is.Null);

    [TestCase("")]
    [TestCase("   ")]
    public void Ctor_WithBlankMapsUrl_LeavesItNull(string mapsUrl) =>
        Assert.That(new Venue("V", "A", PitchSurface.NaturalGrass, mapsUrl).MapsUrl, Is.Null);

    [TestCase("")]
    [TestCase("   ")]
    public void Ctor_WithBlankName_Throws(string name) =>
        Assert.That(() => new Venue(name, "A", PitchSurface.Earth), Throws.ArgumentException);

    [TestCase("")]
    [TestCase("   ")]
    public void Ctor_WithBlankAddress_Throws(string address) =>
        Assert.That(() => new Venue("V", address, PitchSurface.Earth), Throws.ArgumentException);

    [Test]
    public void Ctor_WithDefaultSurface_Throws() =>
        Assert.That(() => new Venue("V", "A", PitchSurface.Default), Throws.ArgumentException);

    [Test]
    public void Ctor_WithUndefinedSurface_Throws() =>
        Assert.That(() => new Venue("V", "A", (PitchSurface)99), Throws.ArgumentException);

    [TestCase("not-a-url")]
    [TestCase("ftp://example.com/place")]
    [TestCase("/relative/path")]
    public void Ctor_WithNonHttpMapsUrl_Throws(string mapsUrl) =>
        Assert.That(() => new Venue("V", "A", PitchSurface.Earth, mapsUrl), Throws.ArgumentException);

    [Test]
    public void Ctor_WithOverlongName_Throws() =>
        Assert.That(
            () => new Venue(new string('x', Venue.MaxNameLength + 1), "A", PitchSurface.Earth),
            Throws.ArgumentException);

    [Test]
    public void Equality_IsByValue()
    {
        var a = new Venue("V", "A", PitchSurface.Hybrid, "https://x.example");
        var b = new Venue("V", "A", PitchSurface.Hybrid, "https://x.example");
        var c = new Venue("V", "A", PitchSurface.Earth, "https://x.example");

        Assert.Multiple(() =>
        {
            Assert.That(a, Is.EqualTo(b));
            Assert.That(a, Is.Not.EqualTo(c));
        });
    }
}
