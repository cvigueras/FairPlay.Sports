using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Application.Teams.LeaveTeam;
using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.TestSupport.Teams;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Teams.LeaveTeam;

[TestFixture]
public class LeaveTeamHandlerTests
{
    private ITeamMemberRepository _members = null!;
    private LeaveTeamHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _members = Substitute.For<ITeamMemberRepository>();
        _handler = new LeaveTeamHandler(_members);
    }

    [Test]
    public async Task Handle_WhenMembershipMissing_ReturnsNotFound()
    {
        _members.GetByTeamAndUserForUpdateAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((TeamMember?)null);

        var result = await _handler.Handle(
            new LeaveTeamCommand(TeamMemberMother.TeamId, TeamMemberMother.UserId), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
        });
        _members.DidNotReceive().Remove(Arg.Any<TeamMember>());
    }

    [Test]
    public async Task Handle_WhenMembershipExists_RemovesIt_AndReturnsSuccess()
    {
        var member = TeamMemberMother.DomainTeamMember();
        _members.GetByTeamAndUserForUpdateAsync(member.TeamId, member.UserId, Arg.Any<CancellationToken>())
            .Returns(member);

        var result = await _handler.Handle(new LeaveTeamCommand(member.TeamId, member.UserId), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        _members.Received(1).Remove(member);
    }
}
