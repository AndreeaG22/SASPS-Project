using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using Versioning.API.Models;
using Versioning.Application.Commands.CreateVersion;
using Versioning.Application.Queries.GetVersionHistory;

namespace Versioning.API.Endpoints;

public static class VersionEndpoints
{
    public static IEndpointRouteBuilder MapVersionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/versions")
            .WithTags("Versions");

        group.MapPost("/", CreateVersion)
            .DisableAntiforgery()
            .Accepts<CreateVersionRequest>("multipart/form-data")
            .Produces<VersionResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .WithName("CreateVersion")
            .WithSummary("Add a new version to a document")
            .WithDescription("Upload a new version of an existing document with optional notes");

        group.MapGet("/document/{documentId:guid}", GetVersionHistory)
            .Produces<VersionHistoryResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("GetVersionHistory")
            .WithSummary("Get all versions of a document")
            .WithDescription("Retrieve complete version history for a document, ordered from newest to oldest");

        return endpoints;
    }

    private static async Task<IResult> CreateVersion(
        [FromForm] Guid documentId,
        [FromForm] string? notes,
        IFormFile file,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        // TODO: when document is created it needs to insert version 1 in the versioning schema 
        try
        {
            // Read file content
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream, cancellationToken);
            var fileContent = memoryStream.ToArray();

            // Create command
            var command = new CreateVersionCommand(
                DocumentId: documentId,
                FileName: file.FileName,
                FileContent: fileContent,
                ContentType: file.ContentType,
                Notes: notes,
                UserId: "system"
            );

            // Execute command
            var result = await mediator.Send(command, cancellationToken);

            // Map to response
            var response = new VersionResponse(
                Id: result.Id,
                DocumentId: result.DocumentId,
                VersionNumber: result.VersionNumber,
                FileName: result.FileName,
                FileSizeInBytes: result.FileSizeInBytes,
                ContentType: result.ContentType,
                Notes: result.Notes,
                IsCurrent: result.IsCurrent,
                CreatedAt: result.CreatedAt,
                CreatedBy: result.CreatedBy
            );

            return Results.Created($"/api/versions/{response.Id}", response);
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
                title: "An error occurred while creating the version"
            );
        }
    }

    private static async Task<IResult> GetVersionHistory(
        Guid documentId,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetVersionHistoryQuery(documentId);
            var result = await mediator.Send(query, cancellationToken);

            var response = new VersionHistoryResponse(
                DocumentId: result.DocumentId,
                Versions: result.Versions
                    .Select(v => new VersionResponse(
                        Id: v.Id,
                        DocumentId: v.DocumentId,
                        VersionNumber: v.VersionNumber,
                        FileName: v.FileName,
                        FileSizeInBytes: v.FileSizeInBytes,
                        ContentType: v.ContentType,
                        Notes: v.Notes,
                        IsCurrent: v.IsCurrent,
                        CreatedAt: v.CreatedAt,
                        CreatedBy: v.CreatedBy
                    ))
                    .ToList(),
                TotalCount: result.TotalCount
            );

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An error occurred while retrieving version history"
            );
        }
    }
}