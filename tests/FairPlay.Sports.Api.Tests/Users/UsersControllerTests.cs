using FairPlay.Sports.Api.Users;
using FairPlay.Sports.TestSupport.Users;
using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Application.Users.Activate;
using FairPlay.Sports.Application.Users.GetAll;
using FairPlay.Sports.Application.Users.GetById;
using FairPlay.Sports.Application.Users.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace FairPlay.Sports.Api.Tests.Users;


[TestFixture]
public class UsersControllerTests
{
    private ISender _sender = null!;
    private UsersController _controller = null!;
    private CancellationTokenSource cancellationTokenSource = null!;

    [SetUp]
    public void SetUp()
    {
        _sender = Substitute.For<ISender>();
        _controller = new UsersController(_sender);
        cancellationTokenSource = new CancellationTokenSource();
    }

    [TearDown]
    public void TearDown()
    {
        cancellationTokenSource.Dispose();
    }

    [Test]
    public async Task GetAll_DispatchesQuery_AndReturnsOkWithHandlerValue()
    {
        var dtos = new List<UserDto> { UserMother.Dto() };
        _sender.Send(Arg.Any<GetAllUsersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<UserDto>>.Success(dtos));

        var response = await _controller.GetAll(cancellationTokenSource.Token);

        var okResult = response.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.SameAs(dtos));
        await _sender.Received(1).Send(Arg.Any<GetAllUsersQuery>(), cancellationTokenSource.Token);
    }

    [Test]
    public async Task GetById_DispatchesQueryWithRouteId_AndReturnsOkWithDto()
    {
        var dto = UserMother.Dto();
        _sender.Send(Arg.Any<GetUserByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<UserDto>.Success(dto));

        var response = await _controller.GetById(dto.Id, CancellationToken.None);

        var okResult = response.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.SameAs(dto));
        await _sender.Received(1).Send(
            Arg.Is<GetUserByIdQuery>(query => query.Id == dto.Id), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetById_WhenHandlerReturnsNotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _sender.Send(Arg.Any<GetUserByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<UserDto>.NotFound($"User '{id}' was not found."));

        var response = await _controller.GetById(id, CancellationToken.None);

        Assert.That(response.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task Register_MapsRequestFieldsOntoCommand()
    {
        var request = UserRequestMother.RegisterRequest();
        _sender.Send(Arg.Any<RegisterUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<UserDto>.Success(UserMother.Dto()));

        await _controller.Register(request, CancellationToken.None);

        await _sender.Received(1).Send(
            Arg.Is<RegisterUserCommand>(command =>
                command.UserName == request.UserName &&
                command.Email == request.Email &&
                command.Password == request.Password &&
                command.Team == request.Team),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Register_WhenHandlerSucceeds_ReturnsCreatedAtActionPointingToGetById()
    {
        var dto = UserMother.Dto();
        _sender.Send(Arg.Any<RegisterUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<UserDto>.Success(dto));

        var response = await _controller.Register(UserRequestMother.RegisterRequest(), CancellationToken.None);

        var createdResult = response.Result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(createdResult!.ActionName, Is.EqualTo(nameof(UsersController.GetById)));
            Assert.That(createdResult!.RouteValues!["id"], Is.EqualTo(dto.Id));
            Assert.That(createdResult!.Value, Is.SameAs(dto));
        });
    }

    [Test]
    public async Task Register_WhenHandlerFails_ReturnsBadRequest()
    {
        _sender.Send(Arg.Any<RegisterUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<UserDto>.Failure(UserMother.EmailAlreadyRegistered));

        var response = await _controller.Register(UserRequestMother.RegisterRequest(), CancellationToken.None);

        Assert.That(response.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Activate_DispatchesCommandWithRouteId_AndReturnsNoContent()
    {
        var id = Guid.NewGuid();
        _sender.Send(Arg.Any<ActivateUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var response = await _controller.Activate(id, CancellationToken.None);

        Assert.That(response, Is.InstanceOf<NoContentResult>());
        await _sender.Received(1).Send(
            Arg.Is<ActivateUserCommand>(command => command.Id == id), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Activate_WhenHandlerReturnsNotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _sender.Send(Arg.Any<ActivateUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.NotFound($"User '{id}' was not found."));

        var response = await _controller.Activate(id, CancellationToken.None);

        Assert.That(response, Is.InstanceOf<NotFoundObjectResult>());
    }
}
