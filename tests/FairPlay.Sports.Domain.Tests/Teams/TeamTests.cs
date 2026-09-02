using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Domain.Tests.Teams;

[TestFixture]
public class TeamTests
{
    private static readonly DateTime Now = new(2026, 9, 2, 12, 0, 0, DateTimeKind.Utc);

    private static Team Create(
        string name = "FairPlay FC",
        string coach = "Marta Rios",
        string city = "Sevilla",
        FootballType type = FootballType.Futsal,
        Division division = Division.First,
        AgeCategory category = AgeCategory.Under19) =>
        Team.Create(Guid.NewGuid(), name, coach, city, type, division, category, Now);

    [Test]
    public void Create_TrimsTextFields_AndStartsInactiveWithoutCrest()
    {
        var team = Create(name: "  FairPlay FC  ", coach: "  Marta Rios  ", city: "  Sevilla  ");

        Assert.Multiple(() =>
        {
            Assert.That(team.Name, Is.EqualTo("FairPlay FC"));
            Assert.That(team.Coach, Is.EqualTo("Marta Rios"));
            Assert.That(team.City, Is.EqualTo("Sevilla"));
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
            () => Team.Create(Guid.Empty, "n", "c", "city", FootballType.Futsal, Division.First, AgeCategory.Under19, Now),
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
    public void RemoveCrest_ClearsBoth()
    {
        var team = Create();
        team.SetCrest([1], "image/png");

        team.RemoveCrest();

        Assert.Multiple(() =>
        {
            Assert.That(team.HasCrest, Is.False);
            Assert.That(team.Crest, Is.Null);
            Assert.That(team.CrestContentType, Is.Null);
        });
    }

    [Test]
    public void Mutators_ChangeTheExpectedField()
    {
        var team = Create();

        team.Rename("Rivals CF");
        team.ChangeCoach("Leo Pena");
        team.Relocate("Cadiz");
        team.ChangeType(FootballType.Football8);
        team.MoveToDivision(Division.Second);
        team.ChangeCategory(AgeCategory.Under10);
        team.Deactivate();

        Assert.Multiple(() =>
        {
            Assert.That(team.Name, Is.EqualTo("Rivals CF"));
            Assert.That(team.Coach, Is.EqualTo("Leo Pena"));
            Assert.That(team.City, Is.EqualTo("Cadiz"));
            Assert.That(team.Type, Is.EqualTo(FootballType.Football8));
            Assert.That(team.Division, Is.EqualTo(Division.Second));
            Assert.That(team.Category, Is.EqualTo(AgeCategory.Under10));
            Assert.That(team.Active, Is.False);
        });
    }
}
