using FluentValidation;

namespace Versioning.Application.Commands.SetCurrentVersion;

public class SetCurrentVersionCommandValidator : AbstractValidator<SetCurrentVersionCommand>
{
    public SetCurrentVersionCommandValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("Document ID is required");

        RuleFor(x => x.VersionNumber)
            .GreaterThan(0).WithMessage("Version number must be greater than 0");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}