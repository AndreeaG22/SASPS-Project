using MetadataIndexing.Domain.Entities;

namespace MetadataIndexing.Domain.Tests.Entities;

public class SearchDocumentIndexTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateIndex()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var title = "Test Document";
        var description = "Test Description";
        var fileName = "test.pdf";
        var contentType = "application/pdf";
        var fileSize = 1024L;
        var createdBy = "user123";
        var createdAt = DateTime.UtcNow;

        // Act
        var index = SearchDocumentIndex.Create(documentId, title, description, fileName, contentType, fileSize, createdBy, createdAt);

        // Assert
        Assert.NotNull(index);
        Assert.Equal(documentId, index.DocumentId);
        Assert.Equal(title, index.Title);
        Assert.Equal(description, index.Description);
        Assert.Equal(fileName, index.FileName);
        Assert.Equal(contentType, index.ContentType);
        Assert.Equal(fileSize, index.FileSizeInBytes);
        Assert.False(index.IsDeleted);
        Assert.Equal(createdBy, index.CreatedBy);
        Assert.Equal(createdAt, index.CreatedAt);
    }

    [Fact]
    public void UpdateMetadata_ShouldUpdateProperties()
    {
        // Arrange
        var index = SearchDocumentIndex.Create(Guid.NewGuid(), "Original", "Original Desc", "file.pdf", "application/pdf", 1024, "user1", DateTime.UtcNow);
        var newTitle = "Updated Title";
        var newDescription = "Updated Description";
        var updatedBy = "user2";

        // Act
        index.UpdateMetadata(newTitle, newDescription, updatedBy);

        // Assert
        Assert.Equal(newTitle, index.Title);
        Assert.Equal(newDescription, index.Description);
        Assert.Equal(updatedBy, index.UpdatedBy);
        Assert.NotNull(index.UpdatedAt);
    }

    [Fact]
    public void MarkAsDeleted_ShouldSetIsDeletedToTrue()
    {
        // Arrange
        var index = SearchDocumentIndex.Create(Guid.NewGuid(), "Title", "Desc", "file.pdf", "application/pdf", 1024, "user1", DateTime.UtcNow);
        var deletedBy = "user2";

        // Act
        index.MarkAsDeleted(deletedBy);

        // Assert
        Assert.True(index.IsDeleted);
        Assert.Equal(deletedBy, index.UpdatedBy);
        Assert.NotNull(index.UpdatedAt);
    }
}
