using Document.API.Models;
using Document.Application.Commands;
using Document.Application.Commands.CreateDocument;
using Document.Application.Queries.GetDocument;
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


        return endpoints;
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