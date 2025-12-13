using MediatR;
using Shared.Events;
using Versioning.Application.DTOs;
using Versioning.Application.Interfaces;
using Versioning.Domain.Services;

namespace Versioning.Application.Commands.SetCurrentVersion;

public class SetCurrentVersionCommandHandler : IRequestHandler<SetCurrentVersionCommand, VersionDto>
{
    private readonly IVersioningUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly IEventPublisher _eventPublisher;

    public SetCurrentVersionCommandHandler(
        IVersioningUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _eventPublisher = eventPublisher;
    }

    public async Task<VersionDto> Handle(
        SetCurrentVersionCommand request,
        CancellationToken cancellationToken)
    {
        var version = await _unitOfWork.Versions.GetByVersionNumberAsync(
            request.DocumentId,
            request.VersionNumber,
            cancellationToken);

        if (version == null)
        {
            throw new InvalidOperationException(
                $"Version {request.VersionNumber} for document '{request.DocumentId}' not found");
        }

        // Get current version for comparison
        var currentVersion = await _unitOfWork.Versions.GetCurrentVersionAsync(request.DocumentId, cancellationToken);
        var previousCurrentVersionNumber = currentVersion?.VersionNumber ?? version.VersionNumber;

        if (currentVersion?.Id == version.Id)
        {
            // Already the current version, return as-is
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

        // Mark all current versions as not current
        var currentVersions = await _unitOfWork.Versions.GetCurrentVersionsForDocumentAsync(request.DocumentId, cancellationToken);
        foreach (var cv in currentVersions)
        {
            cv.MarkAsNotCurrent();
        }
        _unitOfWork.Versions.UpdateRange(currentVersions);

        // Set this version as current
        version.SetAsCurrent(request.UserId);
        _unitOfWork.Versions.Update(version);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Update file system marker
        await _fileStorageService.UpdateCurrentVersionMarkerAsync(
            request.DocumentId,
            request.VersionNumber,
            cancellationToken);

        // Publish event if version changed
        if (previousCurrentVersionNumber != version.VersionNumber)
        {
            var versionChangedEvent = new VersionChangedEvent(
                DocumentId: version.DocumentId,
                NewCurrentVersionNumber: version.VersionNumber,
                PreviousCurrentVersionNumber: previousCurrentVersionNumber,
                ChangedBy: request.UserId,
                ChangedAt: DateTime.UtcNow
            );

            await _eventPublisher.PublishAsync(versionChangedEvent, cancellationToken);
        }

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
