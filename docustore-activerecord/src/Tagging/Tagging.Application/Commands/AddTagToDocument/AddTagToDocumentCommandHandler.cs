using MediatR;
using Tagging.Application.DTOs;
using Tagging.Domain.Entities;

namespace Tagging.Application.Commands.AddTagToDocument;

public class AddTagToDocumentCommandHandler : IRequestHandler<AddTagToDocumentCommand, DocumentTagDto>
{
    public async Task<DocumentTagDto> Handle(AddTagToDocumentCommand request, CancellationToken cancellationToken)
    {
        // Validate tag exists
        var tagExists = await Tag.Exists(request.TagId, cancellationToken);
        if (!tagExists)
        {
            throw new InvalidOperationException($"Tag with ID '{request.TagId}' not found");
        }

        // Check if association already exists
        var existingAssociation = await DocumentTag.FindByDocumentAndTag(
            request.DocumentId, 
            request.TagId, 
            cancellationToken);
            
        if (existingAssociation != null)
        {
            throw new InvalidOperationException("Tag is already associated with this document");
        }

        var documentTag = DocumentTag.Create(request.DocumentId, request.TagId, request.UserId);
        await documentTag.Save(cancellationToken);

        // Reload to get tag information
        var savedDocumentTag = await DocumentTag.Find(documentTag.Id, cancellationToken);
        if (savedDocumentTag == null)
        {
            throw new InvalidOperationException("Failed to save document tag");
        }

        return new DocumentTagDto(
            Id: savedDocumentTag.Id,
            TagId: savedDocumentTag.TagId,
            TagName: savedDocumentTag.Tag.Name,
            TagDescription: savedDocumentTag.Tag.Description,
            AddedAt: savedDocumentTag.CreatedAt,
            AddedBy: savedDocumentTag.CreatedBy
        );
    }
}
