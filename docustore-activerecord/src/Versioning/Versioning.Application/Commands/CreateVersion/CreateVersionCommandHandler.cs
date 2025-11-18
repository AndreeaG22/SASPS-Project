using MediatR;
using Versioning.Application.DTOs;
using Versioning.Domain.Entities;

namespace Versioning.Application.Commands.CreateVersion;

public class CreateVersionCommandHandler : IRequestHandler<CreateVersionCommand, VersionDto>
{
    public async Task<VersionDto> Handle(CreateVersionCommand request, CancellationToken cancellationToken)
    {
        var version = await VersionEntity.Create(
            documentId: request.DocumentId,
            fileName: request.FileName,
            contentType: request.ContentType,
            notes: request.Notes,
            createdBy: request.UserId,
            cancellationToken: cancellationToken
        );

        await version.UploadAndSave(request.FileContent, cancellationToken);

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