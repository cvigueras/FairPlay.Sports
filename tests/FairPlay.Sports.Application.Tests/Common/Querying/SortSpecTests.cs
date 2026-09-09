using FairPlay.Sports.Application.Common.Querying;

namespace FairPlay.Sports.Application.Tests.Common.Querying;

[TestFixture]
public class SortSpecTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase(" , , ")]
    public void Parse_returnsEmpty_forBlankOrSeparatorsOnly(string? sort)
    {
        Assert.That(SortSpec.Parse(sort), Is.Empty);
    }

    [Test]
    public void Parse_readsDirectionPrefix_andLowercasesField()
    {
        var fields = SortSpec.Parse("Name,-CreatedAt,+City");

        Assert.That(fields, Is.EqualTo(new[]
        {
            new SortField("name", false),
            new SortField("createdat", true),
            new SortField("city", false)
        }));
    }

    [Test]
    public void Parse_trimsWhitespaceAroundTokens()
    {
        var fields = SortSpec.Parse("  name ,  -createdAt  ");

        Assert.That(fields, Is.EqualTo(new[]
        {
            new SortField("name", false),
            new SortField("createdat", true)
        }));
    }
}
