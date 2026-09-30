using FluentValidation;

namespace FairPlay.Sports.Application.Teams.SetVenueAvailable;

public sealed class SetVenueAvailableValidator : AbstractValidator<SetVenueAvailableCommand>
{
    public SetVenueAvailableValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.ActingUserId).NotEmpty();
    }
}
