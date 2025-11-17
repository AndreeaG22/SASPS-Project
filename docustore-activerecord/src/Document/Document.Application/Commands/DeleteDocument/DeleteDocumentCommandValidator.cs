using FluentValidation;

namespace Document.Application.Commands.DeleteDocument;

public class DeleteDocumentCommandValidator: AbstractValidator<DeleteDocumentCommand>
{
    public DeleteDocumentCommandValidator()    
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Document ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}