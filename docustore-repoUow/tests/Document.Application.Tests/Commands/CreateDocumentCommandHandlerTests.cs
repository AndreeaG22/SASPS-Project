using Document.Application.Commands.CreateDocument;
using Document.Application.Interfaces;
using Document.Domain.Entities;
using Document.Domain.Enums;
using Document.Domain.Services;
using Moq;
using Shared.Events;

namespace Document.Application.Tests.Commands;

public class CreateDocumentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IDocumentRepository> _mockDocumentRepository;
    private readonly Mock<IFileStorageService> _mockFileStorageService;
    private readonly Mock<IEventPublisher> _mockEventPublisher;
    private readonly CreateDocumentCommandHandler _handler;

    public CreateDocumentCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockDocumentRepository = new Mock<IDocumentRepository>();
        _mockFileStorageService = new Mock<IFileStorageService>();
        _mockEventPublisher = new Mock<IEventPublisher>();

        _mockUnitOfWork.Setup(x => x.Documents).Returns(_mockDocumentRepository.Object);

        _handler = new CreateDocumentCommandHandler(
            _mockUnitOfWork.Object,
            _mockFileStorageService.Object,
            _mockEventPublisher.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateDocument()
    {
        // Arrange
        var command = new CreateDocumentCommand("Test Document", "Description", "test.pdf", new byte[1024], "application/pdf", "user123");
        var expectedFilePath = "/uploads/documents/test.pdf";

        _mockFileStorageService
            .Setup(x => x.CreateDocumentFolderAsync(It.IsAny<Guid>(), command.FileName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFilePath);

        _mockDocumentRepository
            .Setup(x => x.AddAsync(It.IsAny<DocumentEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockEventPublisher
            .Setup(x => x.PublishAsync(It.IsAny<DocumentCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.Title, result.Title);
        Assert.Equal(command.Description, result.Description);
        Assert.Equal(command.FileName, result.FileName);
        Assert.Equal(command.ContentType, result.ContentType);
        Assert.Equal(DocumentStatus.Active, result.Status);
        Assert.Equal(command.UserId, result.CreatedBy);
    }

    [Fact]
    public async Task Handle_ShouldCallFileStorageService()
    {
        // Arrange
        var command = new CreateDocumentCommand("Test", "Desc", "file.pdf", new byte[1024], "application/pdf", "user123");
        _mockFileStorageService.Setup(x => x.CreateDocumentFolderAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("/path");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockFileStorageService.Verify(x => x.CreateDocumentFolderAsync(It.IsAny<Guid>(), command.FileName, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPublishEvent()
    {
        // Arrange
        var command = new CreateDocumentCommand("Test", "Desc", "file.pdf", new byte[1024], "application/pdf", "user123");
        _mockFileStorageService.Setup(x => x.CreateDocumentFolderAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("/path");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockEventPublisher.Verify(x => x.PublishAsync(It.IsAny<DocumentCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyTitle_ShouldThrowException()
    {
        // Arrange
        var command = new CreateDocumentCommand("", "Desc", "file.pdf", new byte[1024], "application/pdf", "user123");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithOversizedFile_ShouldThrowException()
    {
        // Arrange
        var largeFile = new byte[(20 * 1024 * 1024) + 1];
        var command = new CreateDocumentCommand("Test", "Desc", "large.pdf", largeFile, "application/pdf", "user123");
        _mockFileStorageService.Setup(x => x.CreateDocumentFolderAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("/path");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
