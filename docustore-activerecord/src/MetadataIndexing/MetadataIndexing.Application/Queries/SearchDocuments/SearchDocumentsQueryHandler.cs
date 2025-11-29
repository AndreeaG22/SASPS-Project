using MediatR;
using MetadataIndexing.Application.DTOs;
using MetadataIndexing.Domain.Services;

namespace MetadataIndexing.Application.Queries.SearchDocuments;

public class SearchDocumentsQueryHandler : IRequestHandler<SearchDocumentsQuery, SearchResultDto>
{
    private readonly IDocumentSearchService _searchService;

    public SearchDocumentsQueryHandler(IDocumentSearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<SearchResultDto> Handle(SearchDocumentsQuery request, CancellationToken cancellationToken)
    {
        var criteria = new SearchCriteria
        {
            Query = request.Query,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            Creator = request.Creator,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            Page = request.Page,
            PageSize = request.PageSize
        };

        var result = await _searchService.SearchAsync(criteria, cancellationToken);

        return new SearchResultDto(
            Documents: result.Documents.Select(d => new SearchDocumentDto(
                Id: d.Id,
                DocumentId: d.DocumentId,
                Title: d.Title,
                Description: d.Description,
                FileName: d.FileName,
                ContentType: d.ContentType,
                FileSizeInBytes: d.FileSizeInBytes,
                CreatedAt: d.CreatedAt,
                CreatedBy: d.CreatedBy,
                UpdatedAt: d.UpdatedAt,
                UpdatedBy: d.UpdatedBy
            )).ToList(),
            TotalCount: result.TotalCount,
            TotalPages: result.TotalPages,
            CurrentPage: result.CurrentPage,
            PageSize: result.PageSize
        );
    }
}
