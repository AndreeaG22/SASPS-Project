using Document.Application.Commands.DeleteDocument;
using Document.Application.Interfaces;
using Document.Domain.Entities;
using Document.Domain.Enums;
using MediatR;
using Moq;

namespace Document.Application.Tests.Commands;

public class DeleteDocumentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IDocumentRepository> _mockDocumentRepository;
    private readonly DeleteDocumentCommandHandler _handler;

    public DeleteDocumentCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockDocumentRepository = new Mock<IDocumentRepository>();
        _mockUnitOfWork.Setup(x => x.Documents).Returns(_mockDocumentRepository.Object);
        _handler = new DeleteDocumentCommandHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSoftDeleteDocument()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = DocumentEntity.Create("Test Document", "Description", "file.pdf", "application/pdf", "user1");
        var command = new DeleteDocumentCommand(documentId, "user2");

        _mockDocumentRepository
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(Unit.Value, result);
        Assert.Equal(DocumentStatus.Deleted, document.Status);
        _mockDocumentRepository.Verify(x => x.Update(It.IsAny<DocumentEntity>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentDocument_ShouldThrowException()
    {
        // Arrange
        var command = new DeleteDocumentCommand(Guid.NewGuid(), "user1");
        _mockDocumentRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((DocumentEntity?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task Handle_WithAlreadyDeletedDocument_ShouldThrowException()
    {
        // Arrange
        var document = DocumentEntity.Create("Test", "Desc", "file.pdf", "application/pdf", "user1");
        document.SoftDelete("user1");
        var command = new DeleteDocumentCommand(Guid.NewGuid(), "user2");
        _mockDocumentRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(document);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
