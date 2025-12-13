namespace Tagging.Domain.Entities;

/// <summary>
/// Tag entity - persistence ignorant.
/// No direct database access, ORM-specific code, or persistence logic.
/// </summary>
public class Tag
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

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
            Description = string.IsNullOrWhiteSpace(description) ? null : description,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        return tag;
    }
}
