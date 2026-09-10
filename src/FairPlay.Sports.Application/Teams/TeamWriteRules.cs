using FairPlay.Sports.Domain.Teams;
using FluentValidation;

namespace FairPlay.Sports.Application.Teams;

/// <summary>
/// The structural rules shared by <c>CreateTeamValidator</c> and
/// <c>UpdateTeamValidator</c>. Keeps the two use cases in lock-step and out of
/// the handlers.
/// </summary>
internal static class TeamWriteRules
{
    public static void AddTeamWriteRules<T>(this AbstractValidator<T> validator, int currentYear)
        where T : ITeamWriteFields
    {
        validator.RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        validator.RuleFor(x => x.Coach).NotEmpty().MaximumLength(100);
        validator.RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        validator.RuleFor(x => x.Type).IsInEnum();
        validator.RuleFor(x => x.Division).IsInEnum();
        validator.RuleFor(x => x.Category).IsInEnum();

        validator.RuleFor(x => x.ShortName).MaximumLength(Team.MaxShortNameLength);

        validator.RuleFor(x => x.FoundedYear!.Value)
            .InclusiveBetween(Team.MinFoundedYear, currentYear)
            .When(x => x.FoundedYear.HasValue);

        // Venue: filling any facet makes name + address + a real surface required.
        validator.When(x => HasAnyVenueField(x), () =>
        {
            validator.RuleFor(x => x.VenueName).NotEmpty().MaximumLength(Venue.MaxNameLength);
            validator.RuleFor(x => x.VenueAddress).NotEmpty().MaximumLength(Venue.MaxAddressLength);
            validator.RuleFor(x => x.VenueSurface)
                .NotNull()
                .Must(surface => surface is not null && surface != PitchSurface.Default && Enum.IsDefined(surface.Value))
                .WithMessage("A valid pitch surface is required.");
        });

        validator.RuleFor(x => x.VenueMapsUrl)
            .MaximumLength(Venue.MaxMapsUrlLength)
            .Must(BeAbsoluteHttpUrl)
            .WithMessage("Maps link must be an absolute http(s) URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.VenueMapsUrl));

        // Kit colours: both or neither.
        validator.When(
            x => !string.IsNullOrWhiteSpace(x.ColorPrimary) || !string.IsNullOrWhiteSpace(x.ColorSecondary),
            () =>
            {
                validator.RuleFor(x => x.ColorPrimary).NotEmpty().MaximumLength(KitColors.MaxColourLength);
                validator.RuleFor(x => x.ColorSecondary).NotEmpty().MaximumLength(KitColors.MaxColourLength);
            });

        validator.RuleFor(x => x.ContactEmail)
            .EmailAddress()
            .MaximumLength(Team.MaxContactEmailLength)
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));

        validator.RuleFor(x => x.ContactPhone).MaximumLength(Team.MaxContactPhoneLength);

        validator.RuleFor(x => x.Website)
            .MaximumLength(Team.MaxWebsiteLength)
            .Must(BeAbsoluteHttpUrl)
            .WithMessage("Website must be an absolute http(s) URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.Website));
    }

    private static bool HasAnyVenueField(ITeamWriteFields x) =>
        !string.IsNullOrWhiteSpace(x.VenueName) ||
        !string.IsNullOrWhiteSpace(x.VenueAddress) ||
        x.VenueSurface is not null and not PitchSurface.Default ||
        !string.IsNullOrWhiteSpace(x.VenueMapsUrl);

    private static bool BeAbsoluteHttpUrl(string? value) =>
        string.IsNullOrWhiteSpace(value) ||
        (Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri) &&
         (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps));
}
