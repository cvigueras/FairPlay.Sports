using FairPlay.Sports.Api.Standings;
using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Common.Querying;
using FairPlay.Sports.Application.Standings;
using FairPlay.Sports.Application.Standings.Create;
using FairPlay.Sports.Application.Standings.Delete;
using FairPlay.Sports.Application.Standings.GetById;
using FairPlay.Sports.Application.Standings.GetPage;
using FairPlay.Sports.Application.Standings.Update;
using FairPlay.Sports.TestSupport.Standings;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace FairPlay.Sports.Api.Tests.Standings;

[TestFixture]
public class StandingsControllerTests
{
    private ISender _sender = null!;
    private StandingsController _controller = null!;
    private CancellationTokenSource _cancellationTokenSource = null!;

    [SetUp]
    public void SetUp()
    {
        _sender = Substitute.For<ISender>();
        _controller = new StandingsController(_sender);
        _cancellationTokenSource = new CancellationTokenSource();
    }

    [TearDown]
    public void TearDown()
    {
        _cancellationTokenSource.Dispose();
    }

    [Test]
    public async Task GetPage_MapsRequestToQuery_AndReturnsOkWithHandlerValue()
    {
        var pageResult = new PagedResult<StandingDto>([StandingMother.Dto()], Page: 2, PageSize: 5, TotalCount: 11);
        _sender.Send(Arg.Any<GetStandingsPageQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<PagedResult<StandingDto>>.Success(pageResult));

        var teamId = Guid.NewGuid();
        var request = new GetStandingsPageRequest { Page = 2, PageSize = 5, Sort = "-points", TeamId = teamId };

        var response = await _controller.GetPage(request, _cancellationTokenSource.Token);

        var okResult = response.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.SameAs(pageResult));
        await _sender.Received(1).Send(
            Arg.Is<GetStandingsPageQuery>(query =>
                query.Page == 2 &&
                query.PageSize == 5 &&
                query.Sort == "-points" &&
                query.Filter.TeamId == teamId),
            _cancellationTokenSource.Token);
    }

    [Test]
    public async Task GetById_DispatchesQueryWithRouteId_AndReturnsOkWithDto()
    {
        var dto = StandingMother.Dto();
        _sender.Send(Arg.Any<GetStandingByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<StandingDto>.Success(dto));

        var response = await _controller.GetById(dto.Id, CancellationToken.None);

        var okResult = response.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.SameAs(dto));
        await _sender.Received(1).Send(
            Arg.Is<GetStandingByIdQuery>(query => query.Id == dto.Id), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetById_WhenHandlerReturnsNotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _sender.Send(Arg.Any<GetStandingByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<StandingDto>.NotFound($"Standing '{id}' was not found."));

        var response = await _controller.GetById(id, CancellationToken.None);

        Assert.That(response.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task Create_MapsRequestFieldsOntoCommand()
    {
        var teamId = Guid.NewGuid();
        var request = StandingRequestMother.CreateRequest(teamId);
        _sender.Send(Arg.Any<CreateStandingCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<StandingDto>.Success(StandingMother.Dto()));

        await _controller.Create(request, CancellationToken.None);

        await _sender.Received(1).Send(
            Arg.Is<CreateStandingCommand>(command =>
                command.TeamId == teamId &&
                command.Points == request.Points &&
                command.Played == request.Played &&
                command.Won == request.Won &&
                command.Drawn == request.Drawn &&
                command.Lost == request.Lost &&
                command.GoalsFor == request.GoalsFor &&
                command.GoalsAgainst == request.GoalsAgainst),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Create_WhenHandlerSucceeds_ReturnsCreatedAtActionPointingToGetById()
    {
        var dto = StandingMother.Dto();
        _sender.Send(Arg.Any<CreateStandingCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<StandingDto>.Success(dto));

        var response = await _controller.Create(StandingRequestMother.CreateRequest(), CancellationToken.None);

        var createdResult = response.Result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(createdResult!.ActionName, Is.EqualTo(nameof(StandingsController.GetById)));
            Assert.That(createdResult!.RouteValues!["id"], Is.EqualTo(dto.Id));
            Assert.That(createdResult!.Value, Is.SameAs(dto));
        });
    }

    [Test]
    public async Task Create_WhenHandlerFails_ReturnsBadRequest()
    {
        _sender.Send(Arg.Any<CreateStandingCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<StandingDto>.Failure("Team already has a standing."));

        var response = await _controller.Create(StandingRequestMother.CreateRequest(), CancellationToken.None);

        Assert.That(response.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Create_WhenHandlerReturnsNotFound_Returns404()
    {
        var teamId = Guid.NewGuid();
        _sender.Send(Arg.Any<CreateStandingCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<StandingDto>.NotFound($"Team '{teamId}' was not found."));

        var response = await _controller.Create(StandingRequestMother.CreateRequest(teamId), CancellationToken.None);

        Assert.That(response.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task Update_MapsRouteIdAndRequestFieldsOntoCommand_AndReturnsOk()
    {
        var id = Guid.NewGuid();
        var request = StandingRequestMother.UpdateRequest();
        var dto = StandingMother.Dto(id);
        _sender.Send(Arg.Any<UpdateStandingCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<StandingDto>.Success(dto));

        var response = await _controller.Update(id, request, CancellationToken.None);

        var okResult = response.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.SameAs(dto));
        await _sender.Received(1).Send(
            Arg.Is<UpdateStandingCommand>(command =>
                command.Id == id &&
                command.Points == request.Points &&
                command.GoalsFor == request.GoalsFor),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Update_WhenHandlerReturnsNotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _sender.Send(Arg.Any<UpdateStandingCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<StandingDto>.NotFound($"Standing '{id}' was not found."));

        var response = await _controller.Update(id, StandingRequestMother.UpdateRequest(), CancellationToken.None);

        Assert.That(response.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task Delete_DispatchesCommandWithRouteId_AndReturnsNoContent()
    {
        var id = Guid.NewGuid();
        _sender.Send(Arg.Any<DeleteStandingCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var response = await _controller.Delete(id, CancellationToken.None);

        Assert.That(response, Is.InstanceOf<NoContentResult>());
        await _sender.Received(1).Send(
            Arg.Is<DeleteStandingCommand>(command => command.Id == id), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Delete_WhenHandlerReturnsNotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _sender.Send(Arg.Any<DeleteStandingCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.NotFound($"Standing '{id}' was not found."));

        var response = await _controller.Delete(id, CancellationToken.None);

        Assert.That(response, Is.InstanceOf<NotFoundObjectResult>());
    }
}
