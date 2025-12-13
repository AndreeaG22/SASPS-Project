using Microsoft.EntityFrameworkCore;
using Versioning.Application.Interfaces;
using Versioning.Domain.Entities;
using Versioning.Infrastructure.Data;

namespace Versioning.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Version entity operations.
/// </summary>
public class VersionRepository : IVersionRepository
{
    private readonly VersioningDbContext _context;

    public VersionRepository(VersioningDbContext context)
    {
        _context = context;
    }

    public async Task<VersionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Versions.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<VersionEntity?> GetByVersionNumberAsync(Guid documentId, int versionNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Versions
            .Where(v => v.DocumentId == documentId && v.VersionNumber == versionNumber)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<VersionEntity?> GetCurrentVersionAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _context.Versions
            .Where(v => v.DocumentId == documentId && v.IsCurrent)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<VersionEntity>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _context.Versions
            .Where(v => v.DocumentId == documentId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<VersionEntity>> GetCurrentVersionsForDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _context.Versions
            .Where(v => v.DocumentId == documentId && v.IsCurrent)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetNextVersionNumberAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var maxVersion = await _context.Versions
            .Where(v => v.DocumentId == documentId)
            .MaxAsync(v => (int?)v.VersionNumber, cancellationToken);

        return (maxVersion ?? 0) + 1;
    }

    public async Task<int> CountByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _context.Versions
            .CountAsync(v => v.DocumentId == documentId, cancellationToken);
    }

    public async Task AddAsync(VersionEntity version, CancellationToken cancellationToken = default)
    {
        await _context.Versions.AddAsync(version, cancellationToken);
    }

    public void Update(VersionEntity version)
    {
        _context.Versions.Update(version);
    }

    public void UpdateRange(IEnumerable<VersionEntity> versions)
    {
        _context.Versions.UpdateRange(versions);
    }
}
