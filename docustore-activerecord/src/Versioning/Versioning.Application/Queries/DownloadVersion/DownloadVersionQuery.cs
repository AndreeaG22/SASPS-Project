using MediatR;
using Versioning.Application.DTOs;

namespace Versioning.Application.Queries.DownloadVersion;

public record DownloadVersionQuery(
    Guid DocumentId, 
    int VersionNumber
) : IRequest<VersionDownloadDto?>;