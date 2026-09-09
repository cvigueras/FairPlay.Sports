using FairPlay.Sports.Application.Common.Querying;

namespace FairPlay.Sports.Application.Tests.Common.Querying;

[TestFixture]
public class PagedResultTests
{
    [Test]
    public void TotalPages_roundsUp()
    {
        var page = new PagedResult<int>([1, 2, 3], Page: 1, PageSize: 3, TotalCount: 10);

        Assert.That(page.TotalPages, Is.EqualTo(4));
    }

    [Test]
    public void TotalPages_isZero_whenPageSizeIsZero()
    {
        var page = new PagedResult<int>([], Page: 1, PageSize: 0, TotalCount: 10);

        Assert.That(page.TotalPages, Is.EqualTo(0));
    }

    [TestCase(1, 20, 100, false, true)]
    [TestCase(3, 20, 100, true, true)]
    [TestCase(5, 20, 100, true, false)]
    [TestCase(1, 20, 0, false, false)]
    public void HasPrevious_and_HasNext_reflectPosition(
        int page, int pageSize, int total, bool hasPrevious, bool hasNext)
    {
        var result = new PagedResult<int>([], page, pageSize, total);

        Assert.Multiple(() =>
        {
            Assert.That(result.HasPrevious, Is.EqualTo(hasPrevious));
            Assert.That(result.HasNext, Is.EqualTo(hasNext));
        });
    }

    [Test]
    public void Map_projectsItems_andKeepsMetadata()
    {
        var page = new PagedResult<int>([1, 2, 3], Page: 2, PageSize: 3, TotalCount: 9);

        var mapped = page.Map(value => value * 10);

        Assert.Multiple(() =>
        {
            Assert.That(mapped.Items, Is.EqualTo(new[] { 10, 20, 30 }));
            Assert.That(mapped.Page, Is.EqualTo(2));
            Assert.That(mapped.PageSize, Is.EqualTo(3));
            Assert.That(mapped.TotalCount, Is.EqualTo(9));
        });
    }
}
