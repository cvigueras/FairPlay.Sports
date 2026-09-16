using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using MediatR;

namespace FairPlay.Sports.Application.Users.GetTeams;

public sealed record GetUserTeamsQuery(Guid UserId) : IRequest<Result<IReadOnlyList<TeamMemberDto>>>;
