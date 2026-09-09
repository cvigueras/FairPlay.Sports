using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Domain.Tests.Teams;

[TestFixture]
public class TeamTests
{
    private static readonly DateTime Now = new(2026, 9, 2, 12, 0, 0, DateTimeKind.Utc);

    private static TeamClassification Classification(
        FootballType type = FootballType.Futsal,
        Division division = Division.First,
        AgeCategory category = AgeCategory.Juveniles) =>
        new(type, division, category);

    private static Team Create(
        string name = "FairPlay FC",
        string coach = "Marta Rios",
        string city = "Sevilla",
        FootballType type = FootballType.Futsal,
        Division division = Division.First,
        AgeCategory category = AgeCategory.Juveniles) =>
        Team.Create(Guid.NewGuid(), name, coach, city, Classification(type, division, category), Now);

    [Test]
    public void Create_TrimsTextFields_AndStartsInactiveWithoutCrest()
    {
        var team = Create(name: "  FairPlay FC  ", coach: "  Marta Rios  ", city: "  Sevilla  ");

        Assert.Multiple(() =>
        {
            Assert.That(team.Name, Is.EqualTo("FairPlay FC"));
            Assert.That(team.Coach, Is.EqualTo("Marta Rios"));
            Assert.That(team.City, Is.EqualTo("Sevilla"));
            Assert.That(team.Classification, Is.EqualTo(Classification()));
            Assert.That(team.Active, Is.False);
            Assert.That(team.HasCrest, Is.False);
            Assert.That(team.CreatedAt, Is.EqualTo(Now));
        });
    }

    [Test]
    public void Activate_TurnsTheTeamActive()
    {
        var team = Create();

        team.Activate();

        Assert.That(team.Active, Is.True);
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Create_WithBlankName_Throws(string name) =>
        Assert.That(() => Create(name: name), Throws.ArgumentException);

    [TestCase("")]
    [TestCase("   ")]
    public void Create_WithBlankCity_Throws(string city) =>
        Assert.That(() => Create(city: city), Throws.ArgumentException);

    [Test]
    public void Create_WithEmptyId_Throws() =>
        Assert.That(
            () => Team.Create(Guid.Empty, "n", "c", "city", Classification(), Now),
            Throws.ArgumentException);

    [Test]
    public void Create_WithUndefinedEnum_Throws() =>
        Assert.That(
            () => Create(type: (FootballType)99),
            Throws.ArgumentException);

    [Test]
    public void SetCrest_StoresBytes_AndNormalisesContentType()
    {
        var team = Create();

        team.SetCrest([1, 2, 3], "  IMAGE/PNG  ");

        Assert.Multiple(() =>
        {
            Assert.That(team.HasCrest, Is.True);
            Assert.That(team.Crest, Is.EqualTo(new byte[] { 1, 2, 3 }));
            Assert.That(team.CrestContentType, Is.EqualTo("image/png"));
        });
    }

    [Test]
    public void SetCrest_WithEmptyBytes_Throws() =>
        Assert.That(() => Create().SetCrest([], "image/png"), Throws.ArgumentException);

    [Test]
    public void SetCrest_OverMaxSize_Throws() =>
        Assert.That(
            () => Create().SetCrest(new byte[Team.MaxCrestBytes + 1], "image/png"),
            Throws.ArgumentException);

    [Test]
    public void SetCrest_WithUnsupportedContentType_Throws() =>
        Assert.That(() => Create().SetCrest([1], "image/gif"), Throws.ArgumentException);

    [Test]
    public void SetCrest_Twice_ReplacesTheExistingImage()
    {
        var team = Create();
        team.SetCrest([1], "image/png");

        team.SetCrest([2, 2], "image/webp");

        Assert.Multiple(() =>
        {
            Assert.That(team.Crest, Is.EqualTo(new byte[] { 2, 2 }));
            Assert.That(team.CrestContentType, Is.EqualTo("image/webp"));
        });
    }
}
