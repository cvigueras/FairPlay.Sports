using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<AuthResultDto>>;
