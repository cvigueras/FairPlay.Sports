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
        validator.RuleFor(x => x.Category).IsInEnum();

        // Aficionados teams don't compete in divisions - a division is required for every
        // other category, and Aficionados must not be given one.
        validator.RuleFor(x => x.Division)
            .Must(division => division is not null && division != Division.Default && Enum.IsDefined(division.Value))
            .WithMessage("A valid division is required.")
            .When(x => x.Category != AgeCategory.Aficionados);
        validator.RuleFor(x => x.Division)
            .Null()
            .WithMessage("Aficionados teams don't have a division.")
            .When(x => x.Category == AgeCategory.Aficionados);

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

        // Kit: filling any facet (a colour, the shorts colour, or the pattern) makes all four required.
        validator.When(x => HasAnyKitField(x), () =>
        {
            validator.RuleFor(x => x.ColorPrimary).NotEmpty().MaximumLength(KitColors.MaxColourLength);
            validator.RuleFor(x => x.ColorSecondary).NotEmpty().MaximumLength(KitColors.MaxColourLength);
            validator.RuleFor(x => x.ShortsColor).NotEmpty().MaximumLength(KitColors.MaxColourLength);
            validator.RuleFor(x => x.KitPattern)
                .NotNull()
                .Must(pattern => pattern is not null && pattern != KitPattern.Default && Enum.IsDefined(pattern.Value))
                .WithMessage("A valid kit pattern is required.");
        });

        // Alternate kit: same all-or-nothing rule, independent of the main kit.
        validator.When(x => HasAnyAlternateKitField(x), () =>
        {
            validator.RuleFor(x => x.AlternateColorPrimary).NotEmpty().MaximumLength(KitColors.MaxColourLength);
            validator.RuleFor(x => x.AlternateColorSecondary).NotEmpty().MaximumLength(KitColors.MaxColourLength);
            validator.RuleFor(x => x.AlternateShortsColor).NotEmpty().MaximumLength(KitColors.MaxColourLength);
            validator.RuleFor(x => x.AlternateKitPattern)
                .NotNull()
                .Must(pattern => pattern is not null && pattern != KitPattern.Default && Enum.IsDefined(pattern.Value))
                .WithMessage("A valid alternate kit pattern is required.");
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

    private static bool HasAnyKitField(ITeamWriteFields x) =>
        !string.IsNullOrWhiteSpace(x.ColorPrimary) ||
        !string.IsNullOrWhiteSpace(x.ColorSecondary) ||
        !string.IsNullOrWhiteSpace(x.ShortsColor) ||
        x.KitPattern is not null and not KitPattern.Default;

    private static bool HasAnyAlternateKitField(ITeamWriteFields x) =>
        !string.IsNullOrWhiteSpace(x.AlternateColorPrimary) ||
        !string.IsNullOrWhiteSpace(x.AlternateColorSecondary) ||
        !string.IsNullOrWhiteSpace(x.AlternateShortsColor) ||
        x.AlternateKitPattern is not null and not KitPattern.Default;

    private static bool BeAbsoluteHttpUrl(string? value) =>
        string.IsNullOrWhiteSpace(value) ||
        (Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri) &&
         (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps));
}
