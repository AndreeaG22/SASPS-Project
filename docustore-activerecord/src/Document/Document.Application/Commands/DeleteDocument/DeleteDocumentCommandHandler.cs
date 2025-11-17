using Document.Domain.Entities;
using MediatR;

namespace Document.Application.Commands.DeleteDocument;

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, Unit>
{
    public async Task<Unit> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await DocumentEntity.Find(request.Id, cancellationToken);
        
        if (document == null)
        {
            throw new InvalidOperationException($"Document with ID '{request.Id}' not found");
        }
        
        await document.SoftDelete(request.UserId, cancellationToken);
        
        return Unit.Value;
    }
}