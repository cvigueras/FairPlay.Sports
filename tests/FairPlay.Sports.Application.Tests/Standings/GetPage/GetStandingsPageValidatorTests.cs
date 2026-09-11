using FairPlay.Sports.Application.Standings.GetPage;

namespace FairPlay.Sports.Application.Tests.Standings.GetPage;

[TestFixture]
public class GetStandingsPageValidatorTests
{
    private readonly GetStandingsPageValidator _validator = new();

    private static GetStandingsPageQuery Query(int page = 1, int pageSize = 20, string? sort = null) =>
        new(page, pageSize, sort, new StandingFilter());

    [Test]
    public void Passes_forSaneDefaults()
    {
        Assert.That(_validator.Validate(Query()).IsValid, Is.True);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Fails_whenPageIsBelowOne(int page)
    {
        Assert.That(_validator.Validate(Query(page: page)).IsValid, Is.False);
    }

    [TestCase(0)]
    [TestCase(101)]
    public void Fails_whenPageSizeIsOutOfRange(int pageSize)
    {
        Assert.That(_validator.Validate(Query(pageSize: pageSize)).IsValid, Is.False);
    }

    [TestCase("points")]
    [TestCase("-points")]
    [TestCase("goaldifference,-points")]
    [TestCase("POINTS")]
    public void Passes_forWhitelistedSortFields(string sort)
    {
        Assert.That(_validator.Validate(Query(sort: sort)).IsValid, Is.True);
    }

    [TestCase("teamName")]
    [TestCase("points,unknown")]
    [TestCase("-id")]
    public void Fails_forUnknownSortField(string sort)
    {
        Assert.That(_validator.Validate(Query(sort: sort)).IsValid, Is.False);
    }
}
