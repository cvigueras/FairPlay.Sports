using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Domain.Teams;
using MediatR;

namespace FairPlay.Sports.Application.Teams.Create;

public sealed record CreateTeamCommand(
    string Name,
    string Coach,
    string City,
    FootballType Type,
    Division Division,
    AgeCategory Category) : IRequest<Result<TeamDto>>;
