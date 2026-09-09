using System.Text;
using FairPlay.Sports.Api.Teams;
using FairPlay.Sports.TestSupport.Teams;
using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Application.Teams.Activate;
using FairPlay.Sports.Application.Teams.Create;
using FairPlay.Sports.Application.Common.Querying;
using FairPlay.Sports.Application.Teams.GetPage;
using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.Application.Teams.GetById;
using FairPlay.Sports.Application.Teams.GetCrest;
using FairPlay.Sports.Application.Teams.UploadCrest;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace FairPlay.Sports.Api.Tests.Teams;


[TestFixture]
public class TeamsControllerTests
{
    private ISender _sender = null!;
    private TeamsController _controller = null!;
    private CancellationTokenSource cancellationTokenSource = null!;

    [SetUp]
    public void SetUp()
    {
        _sender = Substitute.For<ISender>();
        _controller = new TeamsController(_sender);
        cancellationTokenSource = new CancellationTokenSource();
    }

    [TearDown]
    public void TearDown()
    {
        cancellationTokenSource.Dispose();
    }

    private static IFormFile FormFile(byte[] content, string contentType, string fileName = "crest.png")
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    [Test]
    public async Task GetPage_MapsRequestToQuery_AndReturnsOkWithHandlerValue()
    {
        var pageResult = new PagedResult<TeamDto>([TeamMother.Dto()], Page: 2, PageSize: 5, TotalCount: 11);
        _sender.Send(Arg.Any<GetTeamsPageQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<PagedResult<TeamDto>>.Success(pageResult));

        var request = new GetTeamsPageRequest
        {
            Page = 2,
            PageSize = 5,
            Sort = "-createdAt",
            Name = "sev",
            Coach = "rios",
            Type = FootballType.Futsal,
            Active = true
        };

        var response = await _controller.GetPage(request, cancellationTokenSource.Token);

        var okResult = response.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.SameAs(pageResult));
        await _sender.Received(1).Send(
            Arg.Is<GetTeamsPageQuery>(query =>
                query.Page == 2 &&
                query.PageSize == 5 &&
                query.Sort == "-createdAt" &&
                query.Filter.Name == "sev" &&
                query.Filter.Coach == "rios" &&
                query.Filter.Type == FootballType.Futsal &&
                query.Filter.Active == true),
            cancellationTokenSource.Token);
    }

    [Test]
    public async Task GetById_DispatchesQueryWithRouteId_AndReturnsOkWithDto()
    {
        var dto = TeamMother.Dto();
        _sender.Send(Arg.Any<GetTeamByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<TeamDto>.Success(dto));

        var response = await _controller.GetById(dto.Id, CancellationToken.None);

        var okResult = response.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.SameAs(dto));
        await _sender.Received(1).Send(
            Arg.Is<GetTeamByIdQuery>(query => query.Id == dto.Id), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetById_WhenHandlerReturnsNotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _sender.Send(Arg.Any<GetTeamByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<TeamDto>.NotFound($"Team '{id}' was not found."));

        var response = await _controller.GetById(id, CancellationToken.None);

        Assert.That(response.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task Create_MapsRequestFieldsOntoCommand()
    {
        var request = TeamRequestMother.CreateRequest();
        _sender.Send(Arg.Any<CreateTeamCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<TeamDto>.Success(TeamMother.Dto()));

        await _controller.Create(request, CancellationToken.None);

        await _sender.Received(1).Send(
            Arg.Is<CreateTeamCommand>(command =>
                command.Name == request.Name &&
                command.Coach == request.Coach &&
                command.City == request.City &&
                command.Type == request.Type &&
                command.Division == request.Division &&
                command.Category == request.Category),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Create_WhenHandlerSucceeds_ReturnsCreatedAtActionPointingToGetById()
    {
        var dto = TeamMother.Dto();
        _sender.Send(Arg.Any<CreateTeamCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<TeamDto>.Success(dto));

        var response = await _controller.Create(TeamRequestMother.CreateRequest(), CancellationToken.None);

        var createdResult = response.Result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(createdResult!.ActionName, Is.EqualTo(nameof(TeamsController.GetById)));
            Assert.That(createdResult!.RouteValues!["id"], Is.EqualTo(dto.Id));
            Assert.That(createdResult!.Value, Is.SameAs(dto));
        });
    }

    [Test]
    public async Task Create_WhenHandlerFails_ReturnsBadRequest()
    {
        _sender.Send(Arg.Any<CreateTeamCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<TeamDto>.Failure(TeamMother.NameAlreadyExists));

        var response = await _controller.Create(TeamRequestMother.CreateRequest(), CancellationToken.None);

        Assert.That(response.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Activate_DispatchesCommandWithRouteId_AndReturnsNoContent()
    {
        var id = Guid.NewGuid();
        _sender.Send(Arg.Any<ActivateTeamCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var response = await _controller.Activate(id, CancellationToken.None);

        Assert.That(response, Is.InstanceOf<NoContentResult>());
        await _sender.Received(1).Send(
            Arg.Is<ActivateTeamCommand>(command => command.Id == id), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Activate_WhenHandlerReturnsNotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _sender.Send(Arg.Any<ActivateTeamCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.NotFound($"Team '{id}' was not found."));

        var response = await _controller.Activate(id, CancellationToken.None);

        Assert.That(response, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task UploadCrest_ReadsTheFile_AndDispatchesCommandWithItsBytesAndContentType()
    {
        var id = Guid.NewGuid();
        var bytes = Encoding.UTF8.GetBytes("fake-png-bytes");
        _sender.Send(Arg.Any<UploadTeamCrestCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var response = await _controller.UploadCrest(id, FormFile(bytes, "image/png"), CancellationToken.None);

        Assert.That(response, Is.InstanceOf<NoContentResult>());
        await _sender.Received(1).Send(
            Arg.Is<UploadTeamCrestCommand>(command =>
                command.TeamId == id &&
                command.ContentType == "image/png" &&
                command.Content.SequenceEqual(bytes)),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task UploadCrest_WhenHandlerReturnsNotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _sender.Send(Arg.Any<UploadTeamCrestCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.NotFound($"Team '{id}' was not found."));

        var response = await _controller.UploadCrest(id, FormFile([1, 2, 3], "image/png"), CancellationToken.None);

        Assert.That(response, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetCrest_WhenPresent_ReturnsFileWithStoredContentType()
    {
        var id = Guid.NewGuid();
        var crest = new TeamCrest(Encoding.UTF8.GetBytes("bytes"), "image/webp");
        _sender.Send(Arg.Any<GetTeamCrestQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<TeamCrest>.Success(crest));

        var response = await _controller.GetCrest(id, CancellationToken.None);

        var fileResult = response as FileContentResult;
        Assert.That(fileResult, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(fileResult!.ContentType, Is.EqualTo("image/webp"));
            Assert.That(fileResult!.FileContents, Is.EqualTo(crest.Content));
        });
    }

    [Test]
    public async Task GetCrest_WhenMissing_Returns404()
    {
        var id = Guid.NewGuid();
        _sender.Send(Arg.Any<GetTeamCrestQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<TeamCrest>.NotFound($"Team '{id}' has no crest."));

        var response = await _controller.GetCrest(id, CancellationToken.None);

        Assert.That(response, Is.InstanceOf<NotFoundObjectResult>());
    }
}
