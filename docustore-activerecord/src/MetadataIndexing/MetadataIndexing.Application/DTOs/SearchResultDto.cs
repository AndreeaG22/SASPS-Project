namespace MetadataIndexing.Application.DTOs;

public record SearchResultDto(
    List<SearchDocumentDto> Documents,
    int TotalCount,
    int TotalPages,
    int CurrentPage,
    int PageSize
);
