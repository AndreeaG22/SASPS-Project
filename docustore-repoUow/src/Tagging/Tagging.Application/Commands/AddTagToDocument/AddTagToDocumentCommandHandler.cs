using MediatR;
using Tagging.Application.DTOs;
using Tagging.Application.Interfaces;
using Tagging.Domain.Entities;

namespace Tagging.Application.Commands.AddTagToDocument;

public class AddTagToDocumentCommandHandler : IRequestHandler<AddTagToDocumentCommand, DocumentTagDto>
{
    private readonly ITaggingUnitOfWork _unitOfWork;

    public AddTagToDocumentCommandHandler(ITaggingUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DocumentTagDto> Handle(AddTagToDocumentCommand request, CancellationToken cancellationToken)
    {
        // Validate tag exists
        var tag = await _unitOfWork.Tags.GetByIdAsync(request.TagId, cancellationToken);
        if (tag == null)
        {
            throw new InvalidOperationException($"Tag with ID '{request.TagId}' not found");
        }

        // Check if association already exists
        var existingAssociation = await _unitOfWork.DocumentTags.GetByDocumentAndTagAsync(
            request.DocumentId,
            request.TagId,
            cancellationToken);

        if (existingAssociation != null)
        {
            throw new InvalidOperationException("Tag is already associated with this document");
        }

        var documentTag = DocumentTag.Create(request.DocumentId, request.TagId, request.UserId);
        
        await _unitOfWork.DocumentTags.AddAsync(documentTag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload to get tag information
        var savedDocumentTag = await _unitOfWork.DocumentTags.GetByIdAsync(documentTag.Id, cancellationToken);
        if (savedDocumentTag == null)
        {
            throw new InvalidOperationException("Failed to save document tag");
        }

        return new DocumentTagDto(
            Id: savedDocumentTag.Id,
            TagId: savedDocumentTag.TagId,
            TagName: tag.Name,
            TagDescription: tag.Description,
            AddedAt: savedDocumentTag.CreatedAt,
            AddedBy: savedDocumentTag.CreatedBy
        );
    }
}
