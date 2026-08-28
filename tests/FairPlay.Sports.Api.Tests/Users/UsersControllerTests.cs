using FairPlay.Sports.Api.Users;
using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Application.Users.GetAll;
using FairPlay.Sports.Application.Users.GetById;
using FairPlay.Sports.Application.Users.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;

namespace FairPlay.Sports.Api.Tests.Users;

/// <summary>
/// Unit tests for UsersController in isolation: MediatR's <see cref="ISender"/> is mocked,
/// so these tests verify only the controller's own responsibility - dispatching the right
/// request (correct type, route id and request-to-command mapping) and translating
/// Result/Result&lt;T&gt; outcomes into the correct HTTP responses. Validation, routing,
/// model binding and persistence are out of scope here.
/// </summary>
[TestFixture]
public class UsersControllerTests
{
    private ISender _sender = null!;
    private UsersController _controller = null!;

    private static UserDto SampleDto(Guid? id = null) =>
        new(id ?? Guid.NewGuid(), "carlos", "carlos@example.com", "FairPlay FC", DateTime.UtcNow, Active: true);

    [SetUp]
    public void SetUp()
    {
        _sender = Substitute.For<ISender>();
        _controller = new UsersController(_sender);
    }

    [Test]
    public async Task GetAll_DispatchesQuery_AndReturnsOkWithHandlerValue()
    {
        using var cts = new CancellationTokenSource();
        IReadOnlyList<UserDto> dtos = new List<UserDto> { SampleDto() };
        _sender.Send(Arg.Any<GetAllUsersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<UserDto>>.Success(dtos));

        var response = await _controller.GetAll(cts.Token);

        var okResult = response.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.SameAs(dtos));
        await _sender.Received(1).Send(Arg.Any<GetAllUsersQuery>(), cts.Token);
    }

    [Test]
    public async Task GetById_DispatchesQueryWithRouteId_AndReturnsOkWithDto()
    {
        var dto = SampleDto();
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
        var request = new RegisterUserRequest("carlos", "carlos@example.com", "Sup3rSecret!", "FairPlay FC");
        _sender.Send(Arg.Any<RegisterUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<UserDto>.Success(SampleDto()));

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
        var dto = SampleDto();
        _sender.Send(Arg.Any<RegisterUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<UserDto>.Success(dto));
        var request = new RegisterUserRequest(dto.UserName, dto.Email, "Sup3rSecret!", dto.Team);

        var response = await _controller.Register(request, CancellationToken.None);

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
            .Returns(Result<UserDto>.Failure("Email 'carlos@example.com' is already registered."));
        var request = new RegisterUserRequest("carlos", "carlos@example.com", "Sup3rSecret!", "FairPlay FC");

        var response = await _controller.Register(request, CancellationToken.None);

        Assert.That(response.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
}
