using MediatR;
using Versioning.Application.DTOs;
using Versioning.Domain.Entities;

namespace Versioning.Application.Queries.GetVersionHistory;

public class GetVersionHistoryQueryHandler : IRequestHandler<GetVersionHistoryQuery, VersionHistoryResult>
{
    public async Task<VersionHistoryResult> Handle(GetVersionHistoryQuery request, CancellationToken cancellationToken)
    {
        var versions = await VersionEntity.GetDocumentVersions(request.DocumentId, cancellationToken);

        var versionDtos = versions.Select(v => new VersionDto(
            Id: v.Id,
            DocumentId: v.DocumentId,
            VersionNumber: v.VersionNumber,
            FileName: v.FileName,
            FileSizeInBytes: v.FileSizeInBytes,
            ContentType: v.ContentType,
            Notes: v.Notes,
            IsCurrent: v.IsCurrent,
            CreatedAt: v.CreatedAt,
            CreatedBy: v.CreatedBy
        )).ToList();

        return new VersionHistoryResult(
            DocumentId: request.DocumentId,
            Versions: versionDtos,
            TotalCount: versionDtos.Count
        );
    }
}