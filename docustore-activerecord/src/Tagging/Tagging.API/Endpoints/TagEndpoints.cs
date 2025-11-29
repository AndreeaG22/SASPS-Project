using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using Tagging.API.Models;
using Tagging.Application.Commands.CreateTag;
using Tagging.Application.Commands.AddTagToDocument;
using Tagging.Application.Commands.RemoveTagFromDocument;
using Tagging.Application.Queries.ListTags;
using Tagging.Application.Queries.GetDocumentTags;
using Tagging.Application.Queries.GetDocumentsByTag;
using Tagging.Application.Queries.GetDocumentsByTags;
using Document.API.Models;

namespace Tagging.API.Endpoints;

public static class TagEndpoints
{
    public static IEndpointRouteBuilder MapTagEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/tags")
            .WithTags("Tags");

        // US-010: Create a Tag
        group.MapPost("/", CreateTag)
            .Produces<TagResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .WithName("CreateTag")
            .WithSummary("Create a new tag")
            .WithDescription("Create a new tag with a unique name and optional description");

        // US-011: List All Available Tags
        group.MapGet("/", ListTags)
            .Produces<List<TagResponse>>(StatusCodes.Status200OK)
            .WithName("ListTags")
            .WithSummary("List all tags")
            .WithDescription("Retrieve all available tags sorted alphabetically by name");

        // US-012: Add Tag to Document
        group.MapPost("/documents/{documentId:guid}/tags", AddTagToDocument)
            .Produces<DocumentTagResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("AddTagToDocument")
            .WithSummary("Add tag to document")
            .WithDescription("Associate a tag with a document");

        // US-013: Remove Tag from Document
        group.MapDelete("/documents/{documentId:guid}/tags/{tagId:guid}", RemoveTagFromDocument)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("RemoveTagFromDocument")
            .WithSummary("Remove tag from document")
            .WithDescription("Remove the association between a tag and a document");

        // US-014: View Document Tags
        group.MapGet("/documents/{documentId:guid}/tags", GetDocumentTags)
            .Produces<List<DocumentTagResponse>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("GetDocumentTags")
            .WithSummary("Get document tags")
            .WithDescription("Retrieve all tags associated with a specific document");

        // US-015: Find Documents by Tag
        group.MapGet("/{tagId:guid}/documents", GetDocumentsByTag)
            .Produces<List<DocumentResponse>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("GetDocumentsByTag")
            .WithSummary("Find documents by tag")
            .WithDescription("Retrieve all documents that have a specific tag");

        // US-015 with multiple tags support
        group.MapGet("/documents", GetDocumentsByTags)
            .Produces<List<DocumentResponse>>(StatusCodes.Status200OK)
            .WithName("GetDocumentsByTags")
            .WithSummary("Find documents by multiple tags")
            .WithDescription("Retrieve all documents that have all specified tags (AND logic)");

        return endpoints;
    }

    private static async Task<IResult> CreateTag(
        [FromServices] IMediator mediator,
        [FromBody] CreateTagRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateTagCommand(request.Name, request.Description, "system");
            var tagDto = await mediator.Send(command, cancellationToken);

            var response = new TagResponse(
                Id: tagDto.Id,
                Name: tagDto.Name,
                Description: tagDto.Description,
                CreatedAt: tagDto.CreatedAt,
                CreatedBy: tagDto.CreatedBy
            );

            return Results.Created($"/api/tags/{tagDto.Id}", response);
        }
        catch (InvalidOperationException ex)
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
                title: "An error occurred while creating the tag"
            );
        }
    }

    private static async Task<IResult> ListTags(
        [FromServices] IMediator mediator,
        [FromQuery] bool includeDocumentCount,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new ListTagsQuery(includeDocumentCount);
            var tags = await mediator.Send(query, cancellationToken);

            var response = tags.Select(tag => new TagResponse(
                Id: tag.Id,
                Name: tag.Name,
                Description: tag.Description,
                CreatedAt: tag.CreatedAt,
                CreatedBy: tag.CreatedBy,
                DocumentCount: tag.DocumentCount
            )).ToList();

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An error occurred while retrieving tags"
            );
        }
    }

    private static async Task<IResult> AddTagToDocument(
        [FromServices] IMediator mediator,
        Guid documentId,
        [FromBody] AddTagToDocumentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new AddTagToDocumentCommand(documentId, request.TagId, "system");
            var documentTagDto = await mediator.Send(command, cancellationToken);

            var response = new DocumentTagResponse(
                Id: documentTagDto.Id,
                TagId: documentTagDto.TagId,
                TagName: documentTagDto.TagName,
                TagDescription: documentTagDto.TagDescription,
                AddedAt: documentTagDto.AddedAt,
                AddedBy: documentTagDto.AddedBy
            );

            return Results.Created($"/api/tags/documents/{documentId}/tags/{request.TagId}", response);
        }
        catch (InvalidOperationException ex)
        {
            var statusCode = ex.Message.Contains("not found") 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            return Results.Json(
                new ErrorResponse(Message: ex.Message, StatusCode: statusCode),
                statusCode: statusCode
            );
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An error occurred while adding tag to document"
            );
        }
    }

    private static async Task<IResult> RemoveTagFromDocument(
        [FromServices] IMediator mediator,
        Guid documentId,
        Guid tagId,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new RemoveTagFromDocumentCommand(documentId, tagId);
            await mediator.Send(command, cancellationToken);
            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new ErrorResponse(
                Message: ex.Message,
                StatusCode: StatusCodes.Status404NotFound
            ));
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An error occurred while removing tag from document"
            );
        }
    }

    private static async Task<IResult> GetDocumentTags(
        [FromServices] IMediator mediator,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetDocumentTagsQuery(documentId);
            var documentTags = await mediator.Send(query, cancellationToken);

            var response = documentTags.Select(dt => new DocumentTagResponse(
                Id: dt.Id,
                TagId: dt.TagId,
                TagName: dt.TagName,
                TagDescription: dt.TagDescription,
                AddedAt: dt.AddedAt,
                AddedBy: dt.AddedBy
            )).ToList();

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An error occurred while retrieving document tags"
            );
        }
    }

    private static async Task<IResult> GetDocumentsByTag(
        [FromServices] IMediator mediator,
        Guid tagId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetDocumentsByTagQuery(tagId);
            var documents = await mediator.Send(query, cancellationToken);

            var response = documents.Select(d => new DocumentResponse(
                Id: d.Id,
                Title: d.Title,
                Description: d.Description,
                FileName: d.FileName,
                FileSizeInBytes: d.FileSizeInBytes,
                ContentType: d.ContentType,
                CreatedAt: d.CreatedAt,
                CreatedBy: d.CreatedBy,
                Status: d.Status
            )).ToList();

            return Results.Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new ErrorResponse(
                Message: ex.Message,
                StatusCode: StatusCodes.Status404NotFound
            ));
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An error occurred while retrieving documents by tag"
            );
        }
    }

    private static async Task<IResult> GetDocumentsByTags(
        [FromServices] IMediator mediator,
        [FromQuery] string tagIds,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tagIds))
            {
                return Results.BadRequest(new ErrorResponse(
                    Message: "At least one tag ID must be provided",
                    StatusCode: StatusCodes.Status400BadRequest
                ));
            }

            // Parse comma-separated tag IDs
            var tagIdList = tagIds.Split(',')
                .Select(id => Guid.TryParse(id.Trim(), out var guid) ? guid : (Guid?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();

            if (tagIdList.Count == 0)
            {
                return Results.BadRequest(new ErrorResponse(
                    Message: "Invalid tag IDs provided",
                    StatusCode: StatusCodes.Status400BadRequest
                ));
            }

            var query = new GetDocumentsByTagsQuery(tagIdList);
            var documents = await mediator.Send(query, cancellationToken);

            var response = documents.Select(d => new DocumentResponse(
                Id: d.Id,
                Title: d.Title,
                Description: d.Description,
                FileName: d.FileName,
                FileSizeInBytes: d.FileSizeInBytes,
                ContentType: d.ContentType,
                CreatedAt: d.CreatedAt,
                CreatedBy: d.CreatedBy,
                Status: d.Status
            )).ToList();

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An error occurred while retrieving documents by tags"
            );
        }
    }
}
