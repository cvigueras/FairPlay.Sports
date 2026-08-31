using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Auth.Refresh;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthResultDto>>;
