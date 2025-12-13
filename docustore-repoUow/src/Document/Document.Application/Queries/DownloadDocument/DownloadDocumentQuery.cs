using Document.Application.DTOs;
using MediatR;

namespace Document.Application.Queries.DownloadDocument;

public record DownloadDocumentQuery(Guid DocumentId) : IRequest<DocumentDownloadDto?>;
