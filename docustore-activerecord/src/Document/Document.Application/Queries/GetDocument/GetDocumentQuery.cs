using Document.Application.DTOs;
using MediatR;

namespace Document.Application.Queries.GetDocument;

public record GetDocumentQuery(Guid Id) : IRequest<DocumentDto?>;
