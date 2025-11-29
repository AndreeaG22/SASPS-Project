using MediatR;
using MetadataIndexing.Application.DTOs;

namespace MetadataIndexing.Application.Queries.SearchDocuments;

public record SearchDocumentsQuery(
    string? Query = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? Creator = null,
    string SortBy = "relevance",
    string SortDirection = "desc",
    int Page = 1,
    int PageSize = 20
) : IRequest<SearchResultDto>;
