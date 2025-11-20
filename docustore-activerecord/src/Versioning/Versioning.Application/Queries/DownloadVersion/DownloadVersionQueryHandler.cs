using MediatR;
using Versioning.Application.DTOs;
using Versioning.Domain.Entities;
using Versioning.Domain.Services;

namespace Versioning.Application.Queries.DownloadVersion;

public class DownloadVersionQueryHandler : IRequestHandler<DownloadVersionQuery, VersionDownloadDto?>
{
    public async Task<VersionDownloadDto?> Handle(
        DownloadVersionQuery request, 
        CancellationToken cancellationToken)
    {
        var version = await VersionEntity.GetVersionByNumber(
            request.DocumentId, 
            request.VersionNumber, 
            cancellationToken);

        if (version == null)
            return null;

        var fileContent = await version.GetFileContent(cancellationToken);

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