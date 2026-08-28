using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using NUnit.Framework;

namespace FairPlay.Sports.Application.Tests.Common.Behaviors;

/// <summary>
/// Covers the cross-cutting MediatR pipeline step that replaced the per-handler validation:
/// it must let valid requests through, short-circuit invalid ones with a failed Result
/// (never an exception), and support both <see cref="Result{T}"/> and non-generic
/// <see cref="Result"/> responses.
/// </summary>
[TestFixture]
public class ValidationBehaviorTests
{
    private static EchoCommand ValidCommand() => new("hello");

    [Test]
    public async Task Handle_WithNoValidatorsRegistered_InvokesNext()
    {
        var behavior = new ValidationBehavior<EchoCommand, Result<string>>(
            Array.Empty<IValidator<EchoCommand>>());
        var expected = Result<string>.Success("hello");
        var nextCalled = false;

        RequestHandlerDelegate<Result<string>> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(expected);
        };

        var result = await behavior.Handle(ValidCommand(), next, CancellationToken.None);

        Assert.That(nextCalled, Is.True);
        Assert.That(result, Is.SameAs(expected));
    }

    [Test]
    public async Task Handle_WithValidRequest_InvokesNext()
    {
        var behavior = new ValidationBehavior<EchoCommand, Result<string>>(
            new IValidator<EchoCommand>[] { new EchoValidator() });
        var expected = Result<string>.Success("hello");

        RequestHandlerDelegate<Result<string>> next = () => Task.FromResult(expected);

        var result = await behavior.Handle(ValidCommand(), next, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
    }

    [Test]
    public async Task Handle_WithInvalidRequest_ShortCircuitsWithValidationFailure()
    {
        var behavior = new ValidationBehavior<EchoCommand, Result<string>>(
            new IValidator<EchoCommand>[] { new EchoValidator() });
        var nextCalled = false;

        RequestHandlerDelegate<Result<string>> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result<string>.Success(null!));
        };

        var result = await behavior.Handle(
            new EchoCommand(""), next, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(nextCalled, Is.False);
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.Validation));
            Assert.That(result.Error, Is.Not.Null.And.Not.Empty);
        });
    }

    [Test]
    public async Task Handle_WithInvalidRequest_AndNonGenericResultResponse_ShortCircuitsWithValidationFailure()
    {
        var behavior = new ValidationBehavior<Ping, Result>(
            [new PingValidator()]);
        var nextCalled = false;

        RequestHandlerDelegate<Result> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Success());
        };

        var result = await behavior.Handle(new Ping(""), next, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(nextCalled, Is.False);
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.Validation));
        });
    }

    private sealed record EchoCommand(string Value) : IRequest<Result<string>>;

    private sealed class EchoValidator : AbstractValidator<EchoCommand>
    {
        public EchoValidator() => RuleFor(x => x.Value).NotEmpty();
    }

    private sealed record Ping(string Value) : IRequest<Result>;

    private sealed class PingValidator : AbstractValidator<Ping>
    {
        public PingValidator() => RuleFor(x => x.Value).NotEmpty();
    }
}
