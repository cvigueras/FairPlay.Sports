using FairPlay.Sports.Application.Teams.GetPage;

namespace FairPlay.Sports.Application.Tests.Teams.GetPage;

[TestFixture]
public class GetTeamsPageValidatorTests
{
    private readonly GetTeamsPageValidator _validator = new();

    private static GetTeamsPageQuery Query(int page = 1, int pageSize = 20, string? sort = null) =>
        new(page, pageSize, sort, new TeamFilter());

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
    [TestCase(5000)]
    public void Fails_whenPageSizeIsOutOfRange(int pageSize)
    {
        Assert.That(_validator.Validate(Query(pageSize: pageSize)).IsValid, Is.False);
    }

    [TestCase("name")]
    [TestCase("-createdAt")]
    [TestCase("city,-name")]
    [TestCase("TYPE")]
    public void Passes_forWhitelistedSortFields(string sort)
    {
        Assert.That(_validator.Validate(Query(sort: sort)).IsValid, Is.True);
    }

    [TestCase("coach")]
    [TestCase("name,unknown")]
    [TestCase("-id")]
    public void Fails_forUnknownSortField(string sort)
    {
        Assert.That(_validator.Validate(Query(sort: sort)).IsValid, Is.False);
    }
}
