using Document.API.Models;
using Document.Application.Commands;
using Document.Application.Commands.CreateDocument;
using Document.Application.Queries.GetDocument;
using Document.Application.Queries.ListDocuments;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Document.API.Endpoints;

public static class DocumentEndpoints
{
    public static IEndpointRouteBuilder MapDocumentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/documents")
            .WithTags("Documents");

        group.MapPost("/", CreateDocument)
            .DisableAntiforgery()
            .Accepts<CreateDocumentRequest>("multipart/form-data")
            .Produces<DocumentResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .WithName("CreateDocument")
            .WithSummary("Create a new document")
            .WithDescription("Upload a new document with title, description, and file");

        group.MapGet("/{id:guid}", GetDocument)
            .Produces<DocumentResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("GetDocument")
            .WithSummary("Get document details")
            .WithDescription("Retrieve complete information about a specific document by its ID");

        group.MapGet("/", ListDocuments)
            .Produces<ListDocumentsResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError)
            .WithName("GetAllDocuments")
            .WithSummary("Get all documents")
            .WithDescription("Retrieve a list of all documents");

        return endpoints;
    }

    private static async Task<IResult> ListDocuments(
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new ListDocumentsQuery();
            var result = await mediator.Send(query, cancellationToken);

            var response = new ListDocumentsResponse(
                Documents: result.Documents
                    .Select(doc => new DocumentResponse(
                            Id: doc.Id,
                            Title: doc.Title,
                            Description: doc.Description,
                            FileName: doc.FileName,
                            FileSizeInBytes: doc.FileSizeInBytes,
                            ContentType: doc.ContentType,
                            CreatedAt: doc.CreatedAt,
                            CreatedBy: doc.CreatedBy
                        )
                    )
                    .ToList(),
                TotalCount: result.TotalCount
            );

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An error occurred while retrieving documents"
            );
        }
        return Results.Problem(
            detail: "An unexpected error occurred",
            statusCode: StatusCodes.Status500InternalServerError,
            title: "An error occurred while retrieving documents"
        );
    }

    private static async Task<IResult> CreateDocument(
        [FromForm] string title,
        [FromForm] string? description,
        IFormFile file,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            // Read file content
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream, cancellationToken);
            var fileContent = memoryStream.ToArray();

            // Create command
            var command = new CreateDocumentCommand(
                Title: title,
                Description: description,
                FileName: file.FileName,
                FileContent: fileContent,
                ContentType: file.ContentType,
                UserId: "system"
            );

            // Execute command
            var result = await mediator.Send(command, cancellationToken);

            // Map to response
            var response = new DocumentResponse(
                Id: result.Id,
                Title: result.Title,
                Description: result.Description,
                FileName: result.FileName,
                FileSizeInBytes: result.FileSizeInBytes,
                ContentType: result.ContentType,
                CreatedAt: result.CreatedAt,
                CreatedBy: result.CreatedBy
            );

            return Results.Created($"/api/documents/{response.Id}", response);
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
                title: "An error occurred while creating the document"
            );
        }
    }


    private static async Task<IResult> GetDocument(
        Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetDocumentQuery(id);
            var result = await mediator.Send(query, cancellationToken);

            if (result == null)
            {
                return Results.NotFound(new ErrorResponse(
                    Message: $"Document with ID '{id}' not found",
                    StatusCode: StatusCodes.Status404NotFound
                ));
            }

            var response = new DocumentResponse(
                Id: result.Id,
                Title: result.Title,
                Description: result.Description,
                FileName: result.FileName,
                FileSizeInBytes: result.FileSizeInBytes,
                ContentType: result.ContentType,
                CreatedAt: result.CreatedAt,
                CreatedBy: result.CreatedBy
            );

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An error occurred while retrieving the document"
            );
        }
    }
}