using Microsoft.EntityFrameworkCore;
using Tagging.Application.Interfaces;
using Tagging.Domain.Entities;
using Tagging.Infrastructure.Data;

namespace Tagging.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Tag entity operations.
/// </summary>
public class TagRepository : ITagRepository
{
    private readonly TaggingDbContext _context;

    public TagRepository(TaggingDbContext context)
    {
        _context = context;
    }

    public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tags.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public async Task<List<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tags.AnyAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .AnyAsync(t => t.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public async Task AddAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        await _context.Tags.AddAsync(tag, cancellationToken);
    }

    public void Update(Tag tag)
    {
        _context.Tags.Update(tag);
    }
}
