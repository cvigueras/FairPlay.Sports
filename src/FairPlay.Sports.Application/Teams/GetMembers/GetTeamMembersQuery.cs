using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Teams.GetMembers;

public sealed record GetTeamMembersQuery(Guid TeamId) : IRequest<Result<IReadOnlyList<TeamMemberDto>>>;
