using MediatR;

namespace MetadataIndexing.Application.Commands.ReindexDocuments;

public record ReindexDocumentsCommand : IRequest<int>;
