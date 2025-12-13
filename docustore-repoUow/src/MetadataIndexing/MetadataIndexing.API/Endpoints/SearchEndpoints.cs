using MediatR;
using MetadataIndexing.API.Models;
using MetadataIndexing.Application.Commands.ReindexDocuments;
using MetadataIndexing.Application.Queries.SearchDocuments;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace MetadataIndexing.API.Endpoints;

public static class SearchEndpoints
{
    public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/search")
            .WithTags("Search");

        // US-016, US-017, US-018, US-019, US-020: Search documents with all filters
        group.MapGet("/documents", SearchDocuments)
            .Produces<SearchResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .WithName("SearchDocuments")
            .WithSummary("Search documents")
            .WithDescription("Search documents by keyword, date range, creator with pagination and sorting");

        // Reindex all documents (admin endpoint)
        group.MapPost("/reindex", ReindexDocuments)
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError)
            .WithName("ReindexDocuments")
            .WithSummary("Reindex all documents")
            .WithDescription("Manually trigger re-indexing of all documents in the search index");

        return endpoints;
    }

    private static async Task<IResult> SearchDocuments(
        [FromServices] IMediator mediator,
        [FromQuery] string? query,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? creator,
        [FromQuery] string sortBy = "relevance",
        [FromQuery] string sortDirection = "desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var searchQuery = new SearchDocumentsQuery(
                Query: query,
                FromDate: fromDate,
                ToDate: toDate,
                Creator: creator,
                SortBy: sortBy,
                SortDirection: sortDirection,
                Page: page,
                PageSize: pageSize
            );

            var result = await mediator.Send(searchQuery, cancellationToken);

            var response = new SearchResponse(
                Documents: result.Documents.Select(d => new SearchDocumentResponse(
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

            return Results.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ErrorResponse(
                Message: ex.Message,
                StatusCode: StatusCodes.Status400BadRequest
            ));
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An error occurred while searching documents"
            );
        }
    }

    private static async Task<IResult> ReindexDocuments(
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new ReindexDocumentsCommand();
            var count = await mediator.Send(command, cancellationToken);

            return Results.Ok(new
            {
                Message = $"Successfully re-indexed {count} documents",
                IndexedCount = count
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An error occurred while re-indexing documents"
            );
        }
    }
}
