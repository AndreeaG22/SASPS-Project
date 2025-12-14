using Document.Application.Queries.ListDocuments;
using Document.Application.Interfaces;
using Document.Domain.Entities;
using Moq;

namespace Document.Application.Tests.Queries;

public class ListDocumentsQueryHandlerTests
{
    private readonly Mock<IDocumentRepository> _mockDocumentRepository;
    private readonly ListDocumentsQueryHandler _handler;

    public ListDocumentsQueryHandlerTests()
    {
        _mockDocumentRepository = new Mock<IDocumentRepository>();
        _handler = new ListDocumentsQueryHandler(_mockDocumentRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllActiveDocuments()
    {
        // Arrange
        var documents = new List<DocumentEntity>
        {
            DocumentEntity.Create("Doc1", "Desc1", "file1.pdf", "application/pdf", "user1"),
            DocumentEntity.Create("Doc2", "Desc2", "file2.pdf", "application/pdf", "user2"),
            DocumentEntity.Create("Doc3", "Desc3", "file3.pdf", "application/pdf", "user3")
        };
        var query = new ListDocumentsQuery();

        _mockDocumentRepository
            .Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(documents);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Documents.Count);
    }

    [Fact]
    public async Task Handle_WithNoDocuments_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new ListDocumentsQuery();
        _mockDocumentRepository.Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DocumentEntity>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Documents);
    }
}
