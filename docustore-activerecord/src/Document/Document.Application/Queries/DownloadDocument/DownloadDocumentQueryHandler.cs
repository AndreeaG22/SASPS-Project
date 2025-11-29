using Document.Application.DTOs;
using Document.Domain.Entities;
using Document.Domain.Enums;
using MediatR;
using Versioning.Domain.Entities;

namespace Document.Application.Queries.DownloadDocument;

public class DownloadDocumentQueryHandler : IRequestHandler<DownloadDocumentQuery, DocumentDownloadDto?>
{
    public async Task<DocumentDownloadDto?> Handle(
        DownloadDocumentQuery request, 
        CancellationToken cancellationToken)
    {
        var document = await DocumentEntity.Find(request.DocumentId, cancellationToken);

        if (document == null || document.Status != DocumentStatus.Active)
            return null;

        var currentVersion = await VersionEntity.GetCurrentVersion(request.DocumentId, cancellationToken);

        if (currentVersion == null)
            return null;

        var fileContent = await currentVersion.GetFileContent(cancellationToken);

        return new DocumentDownloadDto(
            DocumentId: request.DocumentId,
            VersionNumber: currentVersion.VersionNumber,
            FileName: $"{Path.GetFileNameWithoutExtension(currentVersion.FileName)}_v{currentVersion.VersionNumber}{Path.GetExtension(currentVersion.FileName)}",
            FileContent: fileContent,
            ContentType: currentVersion.ContentType,
            FileSizeInBytes: currentVersion.FileSizeInBytes
        );
    }
}