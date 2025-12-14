using Tagging.Domain.Entities;

namespace Tagging.Domain.Tests.Entities;

public class DocumentTagTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateDocumentTag()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var createdBy = "user123";

        // Act
        var documentTag = DocumentTag.Create(documentId, tagId, createdBy);

        // Assert
        Assert.NotNull(documentTag);
        Assert.Equal(documentId, documentTag.DocumentId);
        Assert.Equal(tagId, documentTag.TagId);
        Assert.Equal(createdBy, documentTag.CreatedBy);
        Assert.NotEqual(Guid.Empty, documentTag.Id);
    }

    [Fact]
    public void Create_ShouldSetCreatedAt()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var documentTag = DocumentTag.Create(Guid.NewGuid(), Guid.NewGuid(), "user123");

        // Assert
        var afterCreation = DateTime.UtcNow;
        Assert.True(documentTag.CreatedAt >= beforeCreation);
        Assert.True(documentTag.CreatedAt <= afterCreation);
    }
}
