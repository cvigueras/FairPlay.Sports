using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Application.Users.GetTeams;
using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.Domain.Users;
using FairPlay.Sports.TestSupport.Teams;
using FairPlay.Sports.TestSupport.Users;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Users.GetTeams;

[TestFixture]
public class GetUserTeamsHandlerTests
{
    private IUserRepository _users = null!;
    private ITeamMemberRepository _members = null!;
    private GetUserTeamsHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _users = Substitute.For<IUserRepository>();
        _users.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(UserMother.DomainUser());
        _members = Substitute.For<ITeamMemberRepository>();
        _handler = new GetUserTeamsHandler(_users, _members);
    }

    [Test]
    public async Task Handle_WhenUserDoesNotExist_ReturnsNotFound()
    {
        _users.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _handler.Handle(new GetUserTeamsQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
    }

    [Test]
    public async Task Handle_MapsEveryMembershipToDto()
    {
        var member = TeamMemberMother.DomainTeamMember();
        _members.GetByUserIdAsync(TeamMemberMother.UserId, Arg.Any<CancellationToken>())
            .Returns(new List<TeamMember> { member });

        var result = await _handler.Handle(new GetUserTeamsQuery(TeamMemberMother.UserId), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value!.Single().Id, Is.EqualTo(member.Id));
    }
}
