using FairPlay.Sports.Domain.Teams;
using FluentValidation;

namespace FairPlay.Sports.Application.Teams.UploadCrest;

public sealed class UploadTeamCrestValidator : AbstractValidator<UploadTeamCrestCommand>
{
    private static readonly string[] AllowedContentTypes =
        ["image/png", "image/jpeg", "image/webp", "image/svg+xml"];

    public UploadTeamCrestValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("The crest image is empty.")
            .Must(content => content.Length <= Team.MaxCrestBytes)
            .WithMessage($"The crest image cannot exceed {Team.MaxCrestBytes} bytes.");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(type => AllowedContentTypes.Contains(type.Trim().ToLowerInvariant()))
            .WithMessage("The crest must be a PNG, JPEG, WebP or SVG image.");
    }
}
