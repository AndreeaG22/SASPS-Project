using Document.Application.DTOs;
using MediatR;

namespace Document.Application.Queries.ListDocuments;

public record ListDocumentsQuery() : IRequest<ListDocumentsResult>;
