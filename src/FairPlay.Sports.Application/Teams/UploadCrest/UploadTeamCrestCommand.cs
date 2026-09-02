using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Teams.UploadCrest;

public sealed record UploadTeamCrestCommand(Guid TeamId, byte[] Content, string ContentType)
    : IRequest<Result>;
