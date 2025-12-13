using MediatR;
using Tagging.Application.Interfaces;

namespace Tagging.Application.Commands.RemoveTagFromDocument;

public class RemoveTagFromDocumentCommandHandler : IRequestHandler<RemoveTagFromDocumentCommand>
{
    private readonly ITaggingUnitOfWork _unitOfWork;

    public RemoveTagFromDocumentCommandHandler(ITaggingUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RemoveTagFromDocumentCommand request, CancellationToken cancellationToken)
    {
        var documentTag = await _unitOfWork.DocumentTags.GetByDocumentAndTagAsync(
            request.DocumentId,
            request.TagId,
            cancellationToken);

        if (documentTag == null)
        {
            throw new InvalidOperationException("Tag is not associated with this document");
        }

        _unitOfWork.DocumentTags.Remove(documentTag);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
