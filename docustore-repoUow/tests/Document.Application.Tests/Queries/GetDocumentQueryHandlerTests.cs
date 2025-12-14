using Document.Application.Queries.GetDocument;
using Document.Application.Interfaces;
using Document.Domain.Entities;
using Moq;

namespace Document.Application.Tests.Queries;

public class GetDocumentQueryHandlerTests
{
    private readonly Mock<IDocumentRepository> _mockDocumentRepository;
    private readonly GetDocumentQueryHandler _handler;

    public GetDocumentQueryHandlerTests()
    {
        _mockDocumentRepository = new Mock<IDocumentRepository>();
        _handler = new GetDocumentQueryHandler(_mockDocumentRepository.Object);
    }

    [Fact]
    public async Task Handle_WithExistingDocument_ShouldReturnDocument()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = DocumentEntity.Create("Test Document", "Description", "test.pdf", "application/pdf", "user123");
        var query = new GetDocumentQuery(documentId);

        _mockDocumentRepository
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(document.Title, result.Title);
        Assert.Equal(document.Description, result.Description);
    }

    [Fact]
    public async Task Handle_WithNonExistentDocument_ShouldReturnNull()
    {
        // Arrange
        var query = new GetDocumentQuery(Guid.NewGuid());
        _mockDocumentRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((DocumentEntity?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}
