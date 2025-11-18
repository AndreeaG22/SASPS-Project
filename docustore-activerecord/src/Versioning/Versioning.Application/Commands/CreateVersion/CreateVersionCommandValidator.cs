using FluentValidation;

namespace Versioning.Application.Commands.CreateVersion;

public class CreateVersionCommandValidator : AbstractValidator<CreateVersionCommand>
{
    private const long MaxFileSizeInBytes = 20 * 1024 * 1024; // 20MB
    
    public CreateVersionCommandValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("Document ID is required");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required")
            .MaximumLength(255).WithMessage("File name cannot exceed 255 characters");

        RuleFor(x => x.FileContent)
            .NotEmpty().WithMessage("File content is required")
            .Must(content => content.Length <= MaxFileSizeInBytes)
            .WithMessage($"File size cannot exceed {MaxFileSizeInBytes / 1024 / 1024}MB");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => x.Notes != null);

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}