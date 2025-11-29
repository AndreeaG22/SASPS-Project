using MediatR;
using Tagging.Domain.Entities;

namespace Tagging.Application.Commands.RemoveTagFromDocument;

public class RemoveTagFromDocumentCommandHandler : IRequestHandler<RemoveTagFromDocumentCommand>
{
    public async Task Handle(RemoveTagFromDocumentCommand request, CancellationToken cancellationToken)
    {
        var documentTag = await DocumentTag.FindByDocumentAndTag(
            request.DocumentId, 
            request.TagId, 
            cancellationToken);
            
        if (documentTag == null)
        {
            throw new InvalidOperationException("Tag is not associated with this document");
        }

        await documentTag.Delete(cancellationToken);
    }
}
