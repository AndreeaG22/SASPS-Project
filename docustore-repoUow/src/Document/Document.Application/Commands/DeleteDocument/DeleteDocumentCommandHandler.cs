using Document.Application.Interfaces;
using MediatR;

namespace Document.Application.Commands.DeleteDocument;

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDocumentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(request.Id, cancellationToken);

        if (document == null)
        {
            throw new InvalidOperationException($"Document with ID '{request.Id}' not found");
        }

        // Soft delete via domain entity (business logic in domain)
        document.SoftDelete(request.UserId);

        // Update via repository and save
        _unitOfWork.Documents.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
