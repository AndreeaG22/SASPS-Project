namespace MetadataIndexing.API.Models;

public record SearchResponse(
    List<SearchDocumentResponse> Documents,
    int TotalCount,
    int TotalPages,
    int CurrentPage,
    int PageSize
);
