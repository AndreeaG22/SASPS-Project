using Document.Application.Interfaces;
using Versioning.Application.Interfaces;
using Versioning.Domain.Entities;
using Versioning.Domain.Services;

namespace Versioning.Infrastructure.Services;

/// <summary>
/// Implementation of IVersionQueryService that bridges Document and Versioning modules.
/// </summary>
public class VersionQueryService : IVersionQueryService
{
    private readonly IVersionRepository _versionRepository;
    private readonly IFileStorageService _fileStorageService;

    public VersionQueryService(
        IVersionRepository versionRepository,
        IFileStorageService fileStorageService)
    {
        _versionRepository = versionRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<VersionEntity?> GetCurrentVersionAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _versionRepository.GetCurrentVersionAsync(documentId, cancellationToken);
    }

    public async Task<byte[]> GetVersionFileAsync(Guid documentId, int versionNumber, CancellationToken cancellationToken = default)
    {
        return await _fileStorageService.GetVersionFileAsync(documentId, versionNumber, cancellationToken);
    }
}
