using MetadataIndexing.Domain.Entities;
using MetadataIndexing.Domain.Services;
using MetadataIndexing.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetadataIndexing.Infrastructure.Services;

public class DocumentSearchService : IDocumentSearchService
{
    private readonly MetadataIndexingDbContext _context;

    public DocumentSearchService(MetadataIndexingDbContext context)
    {
        _context = context;
    }

    public async Task<SearchResult> SearchAsync(SearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        // Validate pagination parameters
        if (criteria.Page < 1)
            throw new ArgumentException("Page number must be at least 1", nameof(criteria.Page));

        if (criteria.PageSize < 1 || criteria.PageSize > 100)
            throw new ArgumentException("Page size must be between 1 and 100", nameof(criteria.PageSize));

        // Start with base query - non-deleted documents
        var query = _context.Set<SearchDocumentIndex>()
            .Where(d => !d.IsDeleted)
            .AsQueryable();

        // Apply keyword search (US-016)
        if (!string.IsNullOrWhiteSpace(criteria.Query))
        {
            var searchTerm = criteria.Query.Trim();
            query = query.Where(d =>
                EF.Functions.ILike(d.Title, $"%{searchTerm}%") ||
                (d.Description != null && EF.Functions.ILike(d.Description, $"%{searchTerm}%")));
        }

        // Apply date range filter (US-019)
        if (criteria.FromDate.HasValue)
        {
            query = query.Where(d => d.CreatedAt >= criteria.FromDate.Value);
        }

        if (criteria.ToDate.HasValue)
        {
            // Include the entire day
            var endDate = criteria.ToDate.Value.Date.AddDays(1);
            query = query.Where(d => d.CreatedAt < endDate);
        }

        // Apply creator filter (US-020)
        if (!string.IsNullOrWhiteSpace(criteria.Creator))
        {
            query = query.Where(d => d.CreatedBy == criteria.Creator);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting (US-018)
        query = ApplySorting(query, criteria);

        // Calculate pagination
        var totalPages = (int)Math.Ceiling(totalCount / (double)criteria.PageSize);
        var skip = (criteria.Page - 1) * criteria.PageSize;

        // Apply pagination (US-017)
        var documents = await query
            .Skip(skip)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return new SearchResult
        {
            Documents = documents,
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = criteria.Page,
            PageSize = criteria.PageSize
        };
    }

    private IQueryable<SearchDocumentIndex> ApplySorting(
        IQueryable<SearchDocumentIndex> query,
        SearchCriteria criteria)
    {
        var isAscending = criteria.SortDirection?.ToLower() == "asc";

        return criteria.SortBy?.ToLower() switch
        {
            "createdat" or "created" => isAscending
                ? query.OrderBy(d => d.CreatedAt)
                : query.OrderByDescending(d => d.CreatedAt),

            "updatedat" or "modified" => isAscending
                ? query.OrderBy(d => d.UpdatedAt ?? d.CreatedAt)
                : query.OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt),

            "title" => isAscending
                ? query.OrderBy(d => d.Title)
                : query.OrderByDescending(d => d.Title),

            "creator" => isAscending
                ? query.OrderBy(d => d.CreatedBy).ThenByDescending(d => d.CreatedAt)
                : query.OrderByDescending(d => d.CreatedBy).ThenByDescending(d => d.CreatedAt),

            "relevance" or _ => !string.IsNullOrWhiteSpace(criteria.Query)
                ? ApplyRelevanceSort(query, criteria.Query!)
                : query.OrderByDescending(d => d.CreatedAt) // Default: newest first
        };
    }

    private IQueryable<SearchDocumentIndex> ApplyRelevanceSort(
        IQueryable<SearchDocumentIndex> query,
        string searchTerm)
    {
        // Relevance ranking: exact title match > title contains > description contains
        // In PostgreSQL, we can use CASE expressions for this
        return query.OrderByDescending(d =>
            d.Title.ToLower() == searchTerm.ToLower() ? 3 :
            EF.Functions.ILike(d.Title, $"{searchTerm}") ? 2 :
            EF.Functions.ILike(d.Title, $"%{searchTerm}%") ? 1 : 0
        ).ThenByDescending(d => d.CreatedAt);
    }
}
