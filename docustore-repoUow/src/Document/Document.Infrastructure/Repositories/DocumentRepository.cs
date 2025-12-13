using Document.Application.Interfaces;
using Document.Domain.Entities;
using Document.Domain.Enums;
using Document.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Document.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Document entity operations.
/// </summary>
public class DocumentRepository : IDocumentRepository
{
    private readonly DocumentDbContext _context;

    public DocumentRepository(DocumentDbContext context)
    {
        _context = context;
    }

    public async Task<DocumentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Documents.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<List<DocumentEntity>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Where(d => d.Status == DocumentStatus.Active)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DocumentEntity>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Documents.AnyAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<int> CountActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .CountAsync(d => d.Status == DocumentStatus.Active, cancellationToken);
    }

    public async Task AddAsync(DocumentEntity document, CancellationToken cancellationToken = default)
    {
        await _context.Documents.AddAsync(document, cancellationToken);
    }

    public void Update(DocumentEntity document)
    {
        _context.Documents.Update(document);
    }

    public void Remove(DocumentEntity document)
    {
        _context.Documents.Remove(document);
    }
}
