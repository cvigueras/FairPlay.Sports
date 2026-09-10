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

    /* ---- Profile ("ficha") fields ------------------------------------------ */

    private static TeamProfile FullProfile() =>
        new(
            ShortName: "  FPF  ",
            FoundedYear: 1998,
            HomeVenue: new Venue("Pabellón", "Calle 1", PitchSurface.Indoor, "https://maps.google.com/?q=x"),
            Colors: new KitColors("Blue", "White"),
            ContactEmail: "  INFO@Club.Example  ",
            ContactPhone: "  600 000 000  ",
            Website: "  https://club.example  ");

    [Test]
    public void Create_WithoutProfile_LeavesFichaFieldsEmpty()
    {
        var team = Create();

        Assert.Multiple(() =>
        {
            Assert.That(team.ShortName, Is.Null);
            Assert.That(team.FoundedYear, Is.Null);
            Assert.That(team.HomeVenue, Is.Null);
            Assert.That(team.Colors, Is.Null);
            Assert.That(team.ContactEmail, Is.Null);
            Assert.That(team.ContactPhone, Is.Null);
            Assert.That(team.Website, Is.Null);
        });
    }

    [Test]
    public void Create_WithProfile_TrimsAndNormalisesTheFichaFields()
    {
        var team = Team.Create(Guid.NewGuid(), "FairPlay FC", "Marta Rios", "Sevilla", Classification(), Now, FullProfile());

        Assert.Multiple(() =>
        {
            Assert.That(team.ShortName, Is.EqualTo("FPF"));
            Assert.That(team.FoundedYear, Is.EqualTo(1998));
            Assert.That(team.HomeVenue, Is.EqualTo(new Venue("Pabellón", "Calle 1", PitchSurface.Indoor, "https://maps.google.com/?q=x")));
            Assert.That(team.Colors, Is.EqualTo(new KitColors("Blue", "White")));
            Assert.That(team.ContactEmail, Is.EqualTo("info@club.example"));
            Assert.That(team.ContactPhone, Is.EqualTo("600 000 000"));
            Assert.That(team.Website, Is.EqualTo("https://club.example"));
        });
    }

    [TestCase(1849)]
    [TestCase(2027)]
    public void Create_WithFoundedYearOutOfRange_Throws(int year) =>
        Assert.That(
            () => Team.Create(Guid.NewGuid(), "n", "c", "city", Classification(), Now, new TeamProfile(FoundedYear: year)),
            Throws.ArgumentException);

    [Test]
    public void Create_WithFoundedYearEqualToCreationYear_IsAccepted() =>
        Assert.That(
            Team.Create(Guid.NewGuid(), "n", "c", "city", Classification(), Now, new TeamProfile(FoundedYear: Now.Year)).FoundedYear,
            Is.EqualTo(Now.Year));

    [Test]
    public void Create_WithContactEmailWithoutAtSign_Throws() =>
        Assert.That(
            () => Team.Create(Guid.NewGuid(), "n", "c", "city", Classification(), Now, new TeamProfile(ContactEmail: "not-an-email")),
            Throws.ArgumentException);

    [Test]
    public void Create_WithNonHttpWebsite_Throws() =>
        Assert.That(
            () => Team.Create(Guid.NewGuid(), "n", "c", "city", Classification(), Now, new TeamProfile(Website: "club.example")),
            Throws.ArgumentException);

    [Test]
    public void Update_ReplacesCoreFieldsAndProfile()
    {
        var team = Create();

        team.Update(
            "Newcastle Local",
            "New Coach",
            "Cartagena",
            Classification(FootballType.Football8, Division.Second, AgeCategory.Cadetes),
            FullProfile());

        Assert.Multiple(() =>
        {
            Assert.That(team.Name, Is.EqualTo("Newcastle Local"));
            Assert.That(team.Coach, Is.EqualTo("New Coach"));
            Assert.That(team.City, Is.EqualTo("Cartagena"));
            Assert.That(team.Classification, Is.EqualTo(Classification(FootballType.Football8, Division.Second, AgeCategory.Cadetes)));
            Assert.That(team.ShortName, Is.EqualTo("FPF"));
            Assert.That(team.HomeVenue!.Surface, Is.EqualTo(PitchSurface.Indoor));
            Assert.That(team.Colors!.Primary, Is.EqualTo("Blue"));
        });
    }

    [Test]
    public void Update_WithEmptyProfile_ClearsPreviouslySetFichaFields()
    {
        var team = Team.Create(Guid.NewGuid(), "FairPlay FC", "Marta Rios", "Sevilla", Classification(), Now, FullProfile());

        team.Update("FairPlay FC", "Marta Rios", "Sevilla", Classification(), TeamProfile.Empty);

        Assert.Multiple(() =>
        {
            Assert.That(team.ShortName, Is.Null);
            Assert.That(team.HomeVenue, Is.Null);
            Assert.That(team.Colors, Is.Null);
            Assert.That(team.ContactEmail, Is.Null);
        });
    }

    [Test]
    public void Update_WithBlankName_Throws() =>
        Assert.That(
            () => Create().Update("  ", "c", "city", Classification(), TeamProfile.Empty),
            Throws.ArgumentException);
}
