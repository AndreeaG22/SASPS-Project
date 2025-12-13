using Document.Application.DTOs;
using Document.Application.Interfaces;
using Document.Domain.Enums;
using MediatR;

namespace Document.Application.Queries.DownloadDocument;

public class DownloadDocumentQueryHandler : IRequestHandler<DownloadDocumentQuery, DocumentDownloadDto?>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IVersionQueryService _versionQueryService;

    public DownloadDocumentQueryHandler(
        IDocumentRepository documentRepository,
        IVersionQueryService versionQueryService)
    {
        _documentRepository = documentRepository;
        _versionQueryService = versionQueryService;
    }

    public async Task<DocumentDownloadDto?> Handle(
        DownloadDocumentQuery request,
        CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);

        if (document == null || document.Status != DocumentStatus.Active)
            return null;

        var currentVersion = await _versionQueryService.GetCurrentVersionAsync(request.DocumentId, cancellationToken);

        if (currentVersion == null)
            return null;

        var fileContent = await _versionQueryService.GetVersionFileAsync(
            currentVersion.DocumentId,
            currentVersion.VersionNumber,
            cancellationToken);

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
