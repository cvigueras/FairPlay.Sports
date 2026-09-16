using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Application.Teams.GetMembers;
using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.TestSupport.Teams;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Teams.GetMembers;

[TestFixture]
public class GetTeamMembersHandlerTests
{
    private ITeamRepository _teams = null!;
    private ITeamMemberRepository _members = null!;
    private GetTeamMembersHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _teams = Substitute.For<ITeamRepository>();
        _teams.ExistsByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        _members = Substitute.For<ITeamMemberRepository>();
        _handler = new GetTeamMembersHandler(_teams, _members);
    }

    [Test]
    public async Task Handle_WhenTeamDoesNotExist_ReturnsNotFound()
    {
        _teams.ExistsByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(new GetTeamMembersQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
    }

    [Test]
    public async Task Handle_MapsEveryMemberToDto()
    {
        var member = TeamMemberMother.DomainTeamMember();
        _members.GetByTeamIdAsync(TeamMemberMother.TeamId, Arg.Any<CancellationToken>())
            .Returns(new List<TeamMember> { member });

        var result = await _handler.Handle(new GetTeamMembersQuery(TeamMemberMother.TeamId), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value!.Single().Id, Is.EqualTo(member.Id));
    }
}
