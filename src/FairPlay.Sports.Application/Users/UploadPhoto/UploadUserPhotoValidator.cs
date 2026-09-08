using FairPlay.Sports.Domain.Users;
using FluentValidation;

namespace FairPlay.Sports.Application.Users.UploadPhoto;

public sealed class UploadUserPhotoValidator : AbstractValidator<UploadUserPhotoCommand>
{
    private static readonly string[] AllowedContentTypes =
        ["image/png", "image/jpeg", "image/webp", "image/svg+xml"];

    public UploadUserPhotoValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("The profile photo is empty.")
            .Must(content => content.Length <= User.MaxPhotoBytes)
            .WithMessage($"The profile photo cannot exceed {User.MaxPhotoBytes} bytes.");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(type => AllowedContentTypes.Contains(type.Trim().ToLowerInvariant()))
            .WithMessage("The profile photo must be a PNG, JPEG, WebP or SVG image.");
    }
}
