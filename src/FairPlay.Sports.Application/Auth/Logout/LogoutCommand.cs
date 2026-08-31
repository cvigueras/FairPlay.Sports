using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Auth.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest<Result>;
