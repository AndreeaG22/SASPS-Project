using Document.API.Models;
using Document.Application.Commands;
using Document.Application.Commands.CreateDocument;
using Document.Application.Commands.DeleteDocument;
using Document.Application.Commands.UpdateDocument;
using Document.Application.Queries.DownloadDocument;
using Document.Application.Queries.GetDocument;
using Document.Application.Queries.ListDocuments;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

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
        
        group.MapPut("/{id:guid}", UpdateDocument)
            .Produces<DocumentResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .WithName("UpdateDocument")
            .WithSummary("Update document metadata")
            .WithDescription("Update the title and description of an existing document. File content remains unchanged.");

        group.MapDelete("/{id:guid}", DeleteDocument)
            .Produces<DeleteDocumentResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .WithName("DeleteDocument")
            .WithSummary("Soft delete a document")
            .WithDescription("Mark a document as deleted. The document will no longer appear in normal lists but can still be retrieved by ID for audit purposes.");

        group.MapGet("/{id:guid}/download", DownloadDocument)
            .Produces<FileResult>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("DownloadDocument")
            .WithSummary("Download the current version of a document")
            .WithDescription("Retrieve and download the current version of a document by its ID. Tracks the current version from the versioning module.");

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
                            CreatedBy: doc.CreatedBy,
                            Status: doc.Status.ToString()
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
                CreatedBy: result.CreatedBy,
                Status: result.Status.ToString()
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
                CreatedBy: result.CreatedBy,
                Status: result.Status.ToString()
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
    
    private static async Task<IResult> UpdateDocument(
        Guid id,
        [FromBody] UpdateDocumentRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateDocumentCommand(
                Id: id,
                Title: request.Title,
                Description: request.Description,
                UserId: "system"
            );

            var result = await mediator.Send(command, cancellationToken);

            var response = new DocumentResponse(
                Id: result.Id,
                Title: result.Title,
                Description: result.Description,
                FileName: result.FileName,
                FileSizeInBytes: result.FileSizeInBytes,
                ContentType: result.ContentType,
                CreatedAt: result.CreatedAt,
                CreatedBy: result.CreatedBy,
                Status: result.Status.ToString()
            );

            return Results.Ok(response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new ErrorResponse(
                Message: ex.Message,
                StatusCode: StatusCodes.Status404NotFound
            ));
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
                title: "An error occurred while updating the document"
            );
        }
    }
    
    private static async Task<IResult> DeleteDocument(
        Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteDocumentCommand(
                Id: id,
                UserId: "system"
            );

            await mediator.Send(command, cancellationToken);

            var response = new DeleteDocumentResponse(
                Id: id,
                Message: "Document successfully deleted",
                DeletedAt: DateTime.UtcNow
            );

            return Results.Ok(response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new ErrorResponse(
                Message: ex.Message,
                StatusCode: StatusCodes.Status404NotFound
            ));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already deleted"))
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
                title: "An error occurred while deleting the document"
            );
        }
    }

    private static async Task<IResult> DownloadDocument(
        Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new DownloadDocumentQuery(id);
            var result = await mediator.Send(query, cancellationToken);

            if (result == null)
            {
                return Results.NotFound(new ErrorResponse(
                    Message: $"Document with ID '{id}' not found or has no current version",
                    StatusCode: StatusCodes.Status404NotFound
                ));
            }

            return Results.File(
                fileContents: result.FileContent,
                contentType: result.ContentType,
                fileDownloadName: result.FileName
            );
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An error occurred while downloading the document"
            );
        }
    }
}