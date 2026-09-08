using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Domain.Tests.Teams;

[TestFixture]
public class TeamClassificationTests
{
    [Test]
    public void Ctor_KeepsTheThreeFacets()
    {
        var classification = new TeamClassification(FootballType.BeachSoccer, Division.HonorDivision, AgeCategory.Under12);

        Assert.Multiple(() =>
        {
            Assert.That(classification.Type, Is.EqualTo(FootballType.BeachSoccer));
            Assert.That(classification.Division, Is.EqualTo(Division.HonorDivision));
            Assert.That(classification.Category, Is.EqualTo(AgeCategory.Under12));
        });
    }

    [Test]
    public void Ctor_WithUndefinedType_Throws() =>
        Assert.That(
            () => new TeamClassification((FootballType)99, Division.First, AgeCategory.Under19),
            Throws.ArgumentException);

    [Test]
    public void Ctor_WithUndefinedDivision_Throws() =>
        Assert.That(
            () => new TeamClassification(FootballType.Futsal, (Division)99, AgeCategory.Under19),
            Throws.ArgumentException);

    [Test]
    public void Ctor_WithUndefinedCategory_Throws() =>
        Assert.That(
            () => new TeamClassification(FootballType.Futsal, Division.First, (AgeCategory)99),
            Throws.ArgumentException);

    [Test]
    public void Equality_IsByValue()
    {
        var a = new TeamClassification(FootballType.Futsal, Division.First, AgeCategory.Under19);
        var b = new TeamClassification(FootballType.Futsal, Division.First, AgeCategory.Under19);
        var c = new TeamClassification(FootballType.Futsal, Division.First, AgeCategory.Under16);

        Assert.Multiple(() =>
        {
            Assert.That(a, Is.EqualTo(b));
            Assert.That(a, Is.Not.EqualTo(c));
        });
    }
}
