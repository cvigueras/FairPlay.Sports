using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Teams.GetAll;

public sealed record GetAllTeamsQuery() : IRequest<Result<IReadOnlyList<TeamDto>>>;
