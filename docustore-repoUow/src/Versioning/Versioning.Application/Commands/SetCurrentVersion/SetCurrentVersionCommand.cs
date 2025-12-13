using MediatR;
using Versioning.Application.DTOs;

namespace Versioning.Application.Commands.SetCurrentVersion;

public record SetCurrentVersionCommand(
    Guid DocumentId,
    int VersionNumber,
    string UserId
) : IRequest<VersionDto>;
