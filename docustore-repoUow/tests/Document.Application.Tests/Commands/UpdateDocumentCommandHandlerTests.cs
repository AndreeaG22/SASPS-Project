using Document.Application.Commands.UpdateDocument;
using Document.Application.Interfaces;
using Document.Domain.Entities;
using Document.Domain.Enums;
using Moq;

namespace Document.Application.Tests.Commands;

public class UpdateDocumentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IDocumentRepository> _mockDocumentRepository;
    private readonly UpdateDocumentCommandHandler _handler;

    public UpdateDocumentCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockDocumentRepository = new Mock<IDocumentRepository>();
        _mockUnitOfWork.Setup(x => x.Documents).Returns(_mockDocumentRepository.Object);
        _handler = new UpdateDocumentCommandHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateDocument()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = DocumentEntity.Create("Original", "Original Desc", "file.pdf", "application/pdf", "user1");
        var command = new UpdateDocumentCommand(documentId, "Updated Title", "Updated Desc", "user2");

        _mockDocumentRepository
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.Title, result.Title);
        Assert.Equal(command.Description, result.Description);
        _mockDocumentRepository.Verify(x => x.Update(It.IsAny<DocumentEntity>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentDocument_ShouldThrowException()
    {
        // Arrange
        var command = new UpdateDocumentCommand(Guid.NewGuid(), "Title", "Desc", "user1");
        _mockDocumentRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((DocumentEntity?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task Handle_WithEmptyTitle_ShouldThrowException()
    {
        // Arrange
        var document = DocumentEntity.Create("Original", "Desc", "file.pdf", "application/pdf", "user1");
        var command = new UpdateDocumentCommand(Guid.NewGuid(), "", "Desc", "user2");
        _mockDocumentRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(document);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithLongTitle_ShouldThrowException()
    {
        // Arrange
        var document = DocumentEntity.Create("Original", "Desc", "file.pdf", "application/pdf", "user1");
        var command = new UpdateDocumentCommand(Guid.NewGuid(), new string('a', 201), "Desc", "user2");
        _mockDocumentRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(document);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
