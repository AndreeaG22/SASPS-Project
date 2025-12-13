using MediatR;
using Versioning.Application.DTOs;

namespace Versioning.Application.Queries.GetVersionHistory;

public record GetVersionHistoryQuery(Guid DocumentId) : IRequest<VersionHistoryResult>;
