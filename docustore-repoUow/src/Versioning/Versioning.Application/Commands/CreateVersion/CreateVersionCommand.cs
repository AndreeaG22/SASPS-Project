using MediatR;
using Versioning.Application.DTOs;

namespace Versioning.Application.Commands.CreateVersion;

public record CreateVersionCommand(
    Guid DocumentId,
    string FileName,
    byte[] FileContent,
    string ContentType,
    string? Notes,
    string UserId
) : IRequest<VersionDto>;
