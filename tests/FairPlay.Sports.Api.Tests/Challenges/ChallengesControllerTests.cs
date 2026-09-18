using System.Security.Claims;
using FairPlay.Sports.Api.Challenges;
using FairPlay.Sports.Application.Challenges;
using FairPlay.Sports.Application.Challenges.Accept;
using FairPlay.Sports.Application.Challenges.GetTeamChallenges;
using FairPlay.Sports.Application.Challenges.Reject;
using FairPlay.Sports.Application.Challenges.Send;
using FairPlay.Sports.Application.Common;
using FairPlay.Sports.TestSupport.Challenges;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace FairPlay.Sports.Api.Tests.Challenges;

[TestFixture]
public class ChallengesControllerTests
{
    private static readonly Guid CurrentUserId = Guid.Parse("77777777-7777-7777-7777-777777777777");

    private ISender _sender = null!;
    private ChallengesController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _sender = Substitute.For<ISender>();
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", CurrentUserId.ToString())]))
        };
        _controller = new ChallengesController(_sender)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    [Test]
    public async Task GetForTeam_DispatchesQueryWithTeamId_AndReturnsOkWithHandlerValue()
    {
        var teamId = Guid.NewGuid();
        var dtos = new List<ChallengeDto> { ChallengeMother.Dto() };
        _sender.Send(Arg.Any<GetTeamChallengesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<ChallengeDto>>.Success(dtos));

        var response = await _controller.GetForTeam(teamId, CancellationToken.None);

        Assert.That((response.Result as OkObjectResult)?.Value, Is.SameAs(dtos));
        await _sender.Received(1).Send(
            Arg.Is<GetTeamChallengesQuery>(query => query.TeamId == teamId), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Send_DispatchesCommandWithBodyAndCurrentUser_AndReturnsOkWithHandlerValue()
    {
        var dto = ChallengeMother.Dto();
        _sender.Send(Arg.Any<SendChallengeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<ChallengeDto>.Success(dto));
        var request = new SendChallengeRequest(ChallengeMother.ChallengerTeamId, ChallengeMother.ChallengedTeamId, ChallengeMother.Message);

        var response = await _controller.Send(request, CancellationToken.None);

        Assert.That((response.Result as OkObjectResult)?.Value, Is.SameAs(dto));
        await _sender.Received(1).Send(
            Arg.Is<SendChallengeCommand>(command =>
                command.ChallengerTeamId == request.ChallengerTeamId &&
                command.ChallengedTeamId == request.ChallengedTeamId &&
                command.Message == request.Message &&
                command.ActingUserId == CurrentUserId),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Send_WhenHandlerFails_ReturnsBadRequest()
    {
        _sender.Send(Arg.Any<SendChallengeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<ChallengeDto>.Failure(ChallengeMother.NotAllowedToSend));

        var response = await _controller.Send(
            new SendChallengeRequest(Guid.NewGuid(), Guid.NewGuid(), null), CancellationToken.None);

        Assert.That(response.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Accept_DispatchesCommandWithRouteIdAndCurrentUser_AndReturnsOkWithHandlerValue()
    {
        var challengeId = Guid.NewGuid();
        var dto = ChallengeMother.Dto(id: challengeId);
        _sender.Send(Arg.Any<AcceptChallengeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<ChallengeDto>.Success(dto));

        var response = await _controller.Accept(challengeId, CancellationToken.None);

        Assert.That((response.Result as OkObjectResult)?.Value, Is.SameAs(dto));
        await _sender.Received(1).Send(
            Arg.Is<AcceptChallengeCommand>(command => command.Id == challengeId && command.ActingUserId == CurrentUserId),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Reject_DispatchesCommandWithRouteIdAndCurrentUser_AndReturnsOkWithHandlerValue()
    {
        var challengeId = Guid.NewGuid();
        var dto = ChallengeMother.Dto(id: challengeId);
        _sender.Send(Arg.Any<RejectChallengeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<ChallengeDto>.Success(dto));

        var response = await _controller.Reject(challengeId, CancellationToken.None);

        Assert.That((response.Result as OkObjectResult)?.Value, Is.SameAs(dto));
        await _sender.Received(1).Send(
            Arg.Is<RejectChallengeCommand>(command => command.Id == challengeId && command.ActingUserId == CurrentUserId),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Reject_WhenHandlerReturnsNotFound_Returns404()
    {
        _sender.Send(Arg.Any<RejectChallengeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<ChallengeDto>.NotFound("Challenge was not found."));

        var response = await _controller.Reject(Guid.NewGuid(), CancellationToken.None);

        Assert.That(response.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
}
