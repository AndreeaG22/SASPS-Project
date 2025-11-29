using MetadataIndexing.Domain.Entities;

namespace MetadataIndexing.Domain.Services;

public class SearchCriteria
{
    public string? Query { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Creator { get; set; }
    public string SortBy { get; set; } = "relevance";
    public string SortDirection { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class SearchResult
{
    public List<SearchDocumentIndex> Documents { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
}

public interface IDocumentSearchService
{
    Task<SearchResult> SearchAsync(SearchCriteria criteria, CancellationToken cancellationToken = default);
}
