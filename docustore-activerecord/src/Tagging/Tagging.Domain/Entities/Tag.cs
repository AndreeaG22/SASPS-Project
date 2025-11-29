using Microsoft.EntityFrameworkCore;
using Tagging.Domain.Common;

namespace Tagging.Domain.Entities;

public class Tag : ActiveRecordBase
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    // EF Core requires a parameterless constructor
    private Tag() { }

    public static Tag Create(string name, string? description, string createdBy)
    {
        // Trim whitespace
        name = name?.Trim() ?? string.Empty;
        description = description?.Trim();

        // Validation
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Tag name cannot be empty or whitespace only");

        if (name.Length > 50)
            throw new InvalidOperationException("Tag name cannot exceed 50 characters");

        if (description?.Length > 200)
            throw new InvalidOperationException("Tag description cannot exceed 200 characters");

        var tag = new Tag
        {
            Name = name,
            Description = string.IsNullOrWhiteSpace(description) ? null : description
        };

        tag.SetCreatedBy(createdBy);
        return tag;
    }

    public async Task Save(CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        var dbSet = context.Set<Tag>();

        // Check if entity exists in database
        var exists = await dbSet.AnyAsync(t => t.Id == this.Id, cancellationToken);

        if (exists)
        {
            dbSet.Update(this);
        }
        else
        {
            await dbSet.AddAsync(this, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public static async Task<Tag?> Find(Guid id, CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<Tag>().FindAsync(new object[] { id }, cancellationToken);
    }

    public static async Task<Tag?> FindByName(string name, CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<Tag>()
            .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public static async Task<List<Tag>> All(CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<Tag>()
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public static async Task<bool> Exists(Guid id, CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<Tag>().AnyAsync(t => t.Id == id, cancellationToken);
    }

    public static async Task<bool> ExistsByName(string name, CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<Tag>()
            .AnyAsync(t => t.Name.ToLower() == name.ToLower(), cancellationToken);
    }
}
