using MediatR;
using Versioning.Application.DTOs;
using Versioning.Domain.Entities;

namespace Versioning.Application.Commands.SetCurrentVersion;

public class SetCurrentVersionCommandHandler : IRequestHandler<SetCurrentVersionCommand, VersionDto>
{
    public async Task<VersionDto> Handle(
        SetCurrentVersionCommand request, 
        CancellationToken cancellationToken)
    {
        var version = await VersionEntity.GetVersionByNumber(
            request.DocumentId, 
            request.VersionNumber, 
            cancellationToken);

        if (version == null)
        {
            throw new InvalidOperationException(
                $"Version {request.VersionNumber} for document '{request.DocumentId}' not found");
        }

        await version.SetAsCurrent(request.UserId, cancellationToken);

        return new VersionDto(
            Id: version.Id,
            DocumentId: version.DocumentId,
            VersionNumber: version.VersionNumber,
            FileName: version.FileName,
            FileSizeInBytes: version.FileSizeInBytes,
            ContentType: version.ContentType,
            Notes: version.Notes,
            IsCurrent: version.IsCurrent,
            CreatedAt: version.CreatedAt,
            CreatedBy: version.CreatedBy
        );
    }
}