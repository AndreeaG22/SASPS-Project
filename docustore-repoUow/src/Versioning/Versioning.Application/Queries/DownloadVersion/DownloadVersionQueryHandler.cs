using MediatR;
using Versioning.Application.DTOs;
using Versioning.Application.Interfaces;
using Versioning.Domain.Services;

namespace Versioning.Application.Queries.DownloadVersion;

public class DownloadVersionQueryHandler : IRequestHandler<DownloadVersionQuery, VersionDownloadDto?>
{
    private readonly IVersionRepository _versionRepository;
    private readonly IFileStorageService _fileStorageService;

    public DownloadVersionQueryHandler(
        IVersionRepository versionRepository,
        IFileStorageService fileStorageService)
    {
        _versionRepository = versionRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<VersionDownloadDto?> Handle(
        DownloadVersionQuery request,
        CancellationToken cancellationToken)
    {
        var version = await _versionRepository.GetByVersionNumberAsync(
            request.DocumentId,
            request.VersionNumber,
            cancellationToken);

        if (version == null)
            return null;

        var fileContent = await _fileStorageService.GetVersionFileAsync(
            version.DocumentId,
            version.VersionNumber,
            cancellationToken);

        return new VersionDownloadDto(
            Id: version.Id,
            DocumentId: version.DocumentId,
            VersionNumber: version.VersionNumber,
            FileName: $"{Path.GetFileNameWithoutExtension(version.FileName)}_v{version.VersionNumber}{Path.GetExtension(version.FileName)}",
            FileContent: fileContent,
            ContentType: version.ContentType,
            FileSizeInBytes: version.FileSizeInBytes
        );
    }
}
