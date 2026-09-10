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
    AgeCategory Category,
    string? ShortName = null,
    int? FoundedYear = null,
    string? VenueName = null,
    string? VenueAddress = null,
    PitchSurface? VenueSurface = null,
    string? VenueMapsUrl = null,
    string? ColorPrimary = null,
    string? ColorSecondary = null,
    string? ContactEmail = null,
    string? ContactPhone = null,
    string? Website = null) : IRequest<Result<TeamDto>>, ITeamWriteFields;
