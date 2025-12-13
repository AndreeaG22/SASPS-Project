using Document.Application.Commands;
using FluentValidation;

namespace Document.Application.Commands.CreateDocument;

public class CreateDocumentCommandValidator : AbstractValidator<CreateDocumentCommand>
{
    private const long MaxFileSizeInBytes = 20 * 1024 * 1024; // 20MB
    
    public CreateDocumentCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters")
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("Title cannot be empty or whitespace only");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
            .When(x => x.Description != null);

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required");

        RuleFor(x => x.FileContent)
            .NotEmpty().WithMessage("File content is required")
            .Must(content => content.Length <= MaxFileSizeInBytes)
            .WithMessage($"File size cannot exceed {MaxFileSizeInBytes / 1024 / 1024}MB");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
