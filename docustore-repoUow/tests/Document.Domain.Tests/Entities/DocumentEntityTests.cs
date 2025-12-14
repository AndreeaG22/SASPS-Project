using Document.Domain.Entities;
using Document.Domain.Enums;

namespace Document.Domain.Tests.Entities;

/// <summary>
/// Unit tests for DocumentEntity - testing pure business logic without persistence.
/// Repository/UoW pattern allows testing entities in complete isolation.
/// </summary>
public class DocumentEntityTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldCreateDocument()
    {
        // Arrange
        var title = "Test Document";
        var description = "Test Description";
        var fileName = "test.pdf";
        var contentType = "application/pdf";
        var createdBy = "user123";

        // Act
        var document = DocumentEntity.Create(title, description, fileName, contentType, createdBy);

        // Assert
        Assert.NotNull(document);
        Assert.NotEqual(Guid.Empty, document.Id);
        Assert.Equal(title, document.Title);
        Assert.Equal(description, document.Description);
        Assert.Equal(fileName, document.FileName);
        Assert.Equal(contentType, document.ContentType);
        Assert.Equal(DocumentStatus.Active, document.Status);
        Assert.Equal(createdBy, document.CreatedBy);
        Assert.True(document.CreatedAt <= DateTime.UtcNow);
        Assert.True(document.CreatedAt > DateTime.UtcNow.AddSeconds(-5)); // Created within last 5 seconds
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_WithEmptyOrWhitespaceTitle_ShouldThrowException(string invalidTitle)
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            DocumentEntity.Create(invalidTitle, "Description", "file.pdf", "application/pdf", "user123"));

        Assert.Contains("Title cannot be empty or whitespace only", exception.Message);
    }

    [Fact]
    public void Create_WithNullTitle_ShouldThrowException()
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            DocumentEntity.Create(null!, "Description", "file.pdf", "application/pdf", "user123"));

        Assert.Contains("Title cannot be empty or whitespace only", exception.Message);
    }

    [Fact]
    public void Create_WithTitleExceeding200Characters_ShouldThrowException()
    {
        // Arrange
        var longTitle = new string('a', 201);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            DocumentEntity.Create(longTitle, "Description", "file.pdf", "application/pdf", "user123"));

        Assert.Contains("Title cannot exceed 200 characters", exception.Message);
    }

    [Fact]
    public void Create_WithTitleExactly200Characters_ShouldSucceed()
    {
        // Arrange
        var maxTitle = new string('a', 200);

        // Act
        var document = DocumentEntity.Create(maxTitle, "Description", "file.pdf", "application/pdf", "user123");

        // Assert
        Assert.NotNull(document);
        Assert.Equal(maxTitle, document.Title);
    }

    [Fact]
    public void Create_WithDescriptionExceeding2000Characters_ShouldThrowException()
    {
        // Arrange
        var longDescription = new string('a', 2001);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            DocumentEntity.Create("Title", longDescription, "file.pdf", "application/pdf", "user123"));

        Assert.Contains("Description cannot exceed 2000 characters", exception.Message);
    }

    [Fact]
    public void Create_WithDescriptionExactly2000Characters_ShouldSucceed()
    {
        // Arrange
        var maxDescription = new string('a', 2000);

        // Act
        var document = DocumentEntity.Create("Title", maxDescription, "file.pdf", "application/pdf", "user123");

        // Assert
        Assert.NotNull(document);
        Assert.Equal(maxDescription, document.Description);
    }

    [Fact]
    public void Create_WithNullDescription_ShouldSucceed()
    {
        // Act
        var document = DocumentEntity.Create("Title", null, "file.pdf", "application/pdf", "user123");

        // Assert
        Assert.NotNull(document);
        Assert.Null(document.Description);
    }

    [Fact]
    public void Create_WithEmptyDescription_ShouldSucceed()
    {
        // Act
        var document = DocumentEntity.Create("Title", "", "file.pdf", "application/pdf", "user123");

        // Assert
        Assert.NotNull(document);
        Assert.Equal("", document.Description);
    }

    [Fact]
    public void Create_ShouldSetInitialStatusToActive()
    {
        // Act
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");

        // Assert
        Assert.Equal(DocumentStatus.Active, document.Status);
    }

    [Fact]
    public void Create_ShouldSetCreatedAtToCurrentTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");

        // Assert
        var afterCreation = DateTime.UtcNow;
        Assert.True(document.CreatedAt >= beforeCreation);
        Assert.True(document.CreatedAt <= afterCreation);
    }

    [Fact]
    public void Create_ShouldNotSetUpdatedFields()
    {
        // Act
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");

        // Assert
        Assert.Null(document.UpdatedAt);
        Assert.Null(document.UpdatedBy);
    }

    [Fact]
    public void Create_ShouldNotSetDeletedFields()
    {
        // Act
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");

        // Assert
        Assert.Null(document.DeletedAt);
        Assert.Null(document.DeletedBy);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Act
        var document1 = DocumentEntity.Create("Title 1", "Description", "file1.pdf", "application/pdf", "user123");
        var document2 = DocumentEntity.Create("Title 2", "Description", "file2.pdf", "application/pdf", "user123");

        // Assert
        Assert.NotEqual(document1.Id, document2.Id);
    }

    #endregion

    #region SetFileInfo Tests

    [Fact]
    public void SetFileInfo_WithValidData_ShouldSetFileProperties()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "test.pdf", "application/pdf", "user123");
        var filePath = "/uploads/documents/test.pdf";
        var fileSize = 1024L;

        // Act
        document.SetFileInfo(filePath, fileSize);

        // Assert
        Assert.Equal(filePath, document.FilePathOnDisk);
        Assert.Equal(fileSize, document.FileSizeInBytes);
    }

    [Fact]
    public void SetFileInfo_WithZeroSize_ShouldSucceed()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "test.pdf", "application/pdf", "user123");

        // Act
        document.SetFileInfo("/path/test.pdf", 0);

        // Assert
        Assert.Equal(0, document.FileSizeInBytes);
    }

    [Fact]
    public void SetFileInfo_WithMaxAllowedSize_ShouldSucceed()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "test.pdf", "application/pdf", "user123");
        var maxSize = 20 * 1024 * 1024; // 20MB

        // Act
        document.SetFileInfo("/path/test.pdf", maxSize);

        // Assert
        Assert.Equal(maxSize, document.FileSizeInBytes);
    }

    [Fact]
    public void SetFileInfo_WithSizeExceeding20MB_ShouldThrowException()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "test.pdf", "application/pdf", "user123");
        var oversizedFile = (20 * 1024 * 1024) + 1; // 20MB + 1 byte

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            document.SetFileInfo("/path/test.pdf", oversizedFile));

        Assert.Contains("File size exceeds the maximum allowed size of 20MB", exception.Message);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidData_ShouldUpdateDocument()
    {
        // Arrange
        var document = DocumentEntity.Create("Original Title", "Original Description", "file.pdf", "application/pdf", "user123");
        var originalCreatedAt = document.CreatedAt;
        var newTitle = "Updated Title";
        var newDescription = "Updated Description";
        var updatedBy = "user456";

        // Act
        document.Update(newTitle, newDescription, updatedBy);

        // Assert
        Assert.Equal(newTitle, document.Title);
        Assert.Equal(newDescription, document.Description);
        Assert.Equal(updatedBy, document.UpdatedBy);
        Assert.NotNull(document.UpdatedAt);
        Assert.True(document.UpdatedAt <= DateTime.UtcNow);
        Assert.Equal(originalCreatedAt, document.CreatedAt); // CreatedAt should not change
        Assert.Equal("user123", document.CreatedBy); // CreatedBy should not change
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithEmptyTitle_ShouldThrowException(string invalidTitle)
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            document.Update(invalidTitle, "Description", "user456"));

        Assert.Contains("Title cannot be empty or whitespace only", exception.Message);
    }

    [Fact]
    public void Update_WithNullTitle_ShouldThrowException()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            document.Update(null!, "Description", "user456"));

        Assert.Contains("Title cannot be empty or whitespace only", exception.Message);
    }

    [Fact]
    public void Update_WithTitleExceeding200Characters_ShouldThrowException()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");
        var longTitle = new string('a', 201);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            document.Update(longTitle, "Description", "user456"));

        Assert.Contains("Title cannot exceed 200 characters", exception.Message);
    }

    [Fact]
    public void Update_WithDescriptionExceeding2000Characters_ShouldThrowException()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");
        var longDescription = new string('a', 2001);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            document.Update("Title", longDescription, "user456"));

        Assert.Contains("Description cannot exceed 2000 characters", exception.Message);
    }

    [Fact]
    public void Update_WithNullDescription_ShouldSucceed()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");

        // Act
        document.Update("New Title", null, "user456");

        // Assert
        Assert.Null(document.Description);
    }

    [Fact]
    public void Update_ShouldSetUpdatedAtToCurrentTime()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");
        var beforeUpdate = DateTime.UtcNow;

        // Act
        document.Update("New Title", "New Description", "user456");

        // Assert
        var afterUpdate = DateTime.UtcNow;
        Assert.NotNull(document.UpdatedAt);
        Assert.True(document.UpdatedAt >= beforeUpdate);
        Assert.True(document.UpdatedAt <= afterUpdate);
    }

    [Fact]
    public void Update_MultipleTimes_ShouldUpdateTimestampEachTime()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");

        // Act
        document.Update("Title 1", "Desc 1", "user1");
        var firstUpdateTime = document.UpdatedAt;
        
        Thread.Sleep(10); // Ensure time passes
        
        document.Update("Title 2", "Desc 2", "user2");
        var secondUpdateTime = document.UpdatedAt;

        // Assert
        Assert.NotNull(firstUpdateTime);
        Assert.NotNull(secondUpdateTime);
        Assert.True(secondUpdateTime > firstUpdateTime);
    }

    #endregion

    #region SoftDelete Tests

    [Fact]
    public void SoftDelete_WithActiveDocument_ShouldMarkAsDeleted()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");
        var deletedBy = "user456";

        // Act
        document.SoftDelete(deletedBy);

        // Assert
        Assert.Equal(DocumentStatus.Deleted, document.Status);
        Assert.NotNull(document.DeletedAt);
        Assert.Equal(deletedBy, document.DeletedBy);
        Assert.True(document.DeletedAt <= DateTime.UtcNow);
        Assert.NotNull(document.UpdatedAt);
        Assert.Equal(deletedBy, document.UpdatedBy);
    }

    [Fact]
    public void SoftDelete_WithAlreadyDeletedDocument_ShouldThrowException()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");
        document.SoftDelete("user123");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            document.SoftDelete("user456"));

        Assert.Contains("Document is already deleted", exception.Message);
    }

    [Fact]
    public void SoftDelete_ShouldSetDeletedAtToCurrentTime()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");
        var beforeDelete = DateTime.UtcNow;

        // Act
        document.SoftDelete("user456");

        // Assert
        var afterDelete = DateTime.UtcNow;
        Assert.NotNull(document.DeletedAt);
        Assert.True(document.DeletedAt >= beforeDelete);
        Assert.True(document.DeletedAt <= afterDelete);
    }

    [Fact]
    public void SoftDelete_ShouldPreserveOriginalCreationInfo()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");
        var originalCreatedAt = document.CreatedAt;
        var originalCreatedBy = document.CreatedBy;

        // Act
        document.SoftDelete("user456");

        // Assert
        Assert.Equal(originalCreatedAt, document.CreatedAt);
        Assert.Equal(originalCreatedBy, document.CreatedBy);
    }

    #endregion

    #region Restore Tests

    [Fact]
    public void Restore_WithDeletedDocument_ShouldRestoreDocument()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");
        document.SoftDelete("user123");
        var restoredBy = "user456";

        // Act
        document.Restore(restoredBy);

        // Assert
        Assert.Equal(DocumentStatus.Active, document.Status);
        Assert.Null(document.DeletedAt);
        Assert.Null(document.DeletedBy);
        Assert.Equal(restoredBy, document.UpdatedBy);
        Assert.NotNull(document.UpdatedAt);
    }

    [Fact]
    public void Restore_WithActiveDocument_ShouldThrowException()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            document.Restore("user456"));

        Assert.Contains("Only deleted documents can be restored", exception.Message);
    }

    [Fact]
    public void Restore_ShouldClearDeletedFields()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");
        document.SoftDelete("deleter");

        // Act
        document.Restore("restorer");

        // Assert
        Assert.Null(document.DeletedAt);
        Assert.Null(document.DeletedBy);
    }

    [Fact]
    public void Restore_ShouldSetUpdatedAtToCurrentTime()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");
        document.SoftDelete("deleter");
        var beforeRestore = DateTime.UtcNow;

        // Act
        document.Restore("restorer");

        // Assert
        var afterRestore = DateTime.UtcNow;
        Assert.NotNull(document.UpdatedAt);
        Assert.True(document.UpdatedAt >= beforeRestore);
        Assert.True(document.UpdatedAt <= afterRestore);
    }

    [Fact]
    public void Restore_MultipleTimes_ShouldWork()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");

        // Act & Assert - Delete and restore multiple times
        document.SoftDelete("user1");
        Assert.Equal(DocumentStatus.Deleted, document.Status);

        document.Restore("user2");
        Assert.Equal(DocumentStatus.Active, document.Status);

        document.SoftDelete("user3");
        Assert.Equal(DocumentStatus.Deleted, document.Status);

        document.Restore("user4");
        Assert.Equal(DocumentStatus.Active, document.Status);
    }

    #endregion

    #region Immutability Tests

    [Fact]
    public void Id_ShouldNotBeChangeable()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");
        var originalId = document.Id;

        // Act & Assert
        // Id is private set, so this should not compile if we try to set it
        // This test verifies the property is read-only from outside the class
        Assert.Equal(originalId, document.Id);
    }

    [Fact]
    public void CreatedBy_ShouldNotChangeAfterCreation()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");
        var originalCreatedBy = document.CreatedBy;

        // Act - Perform various operations
        document.Update("New Title", "New Desc", "user456");
        document.SoftDelete("user789");

        // Assert
        Assert.Equal(originalCreatedBy, document.CreatedBy);
    }

    [Fact]
    public void CreatedAt_ShouldNotChangeAfterCreation()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");
        var originalCreatedAt = document.CreatedAt;

        // Act - Perform various operations
        Thread.Sleep(100);
        document.Update("New Title", "New Desc", "user456");

        // Assert
        Assert.Equal(originalCreatedAt, document.CreatedAt);
    }

    #endregion

    #region State Transition Tests

    [Fact]
    public void StateTransition_ActiveToDeleted_ShouldBeAllowed()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");

        // Act & Assert
        Assert.Equal(DocumentStatus.Active, document.Status);
        document.SoftDelete("user123");
        Assert.Equal(DocumentStatus.Deleted, document.Status);
    }

    [Fact]
    public void StateTransition_DeletedToActive_ShouldBeAllowed()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");
        document.SoftDelete("user123");

        // Act & Assert
        Assert.Equal(DocumentStatus.Deleted, document.Status);
        document.Restore("user123");
        Assert.Equal(DocumentStatus.Active, document.Status);
    }

    [Fact]
    public void StateTransition_DeletedToDeleted_ShouldNotBeAllowed()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");
        document.SoftDelete("user123");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => document.SoftDelete("user456"));
    }

    [Fact]
    public void StateTransition_ActiveToActive_ShouldNotBeAllowedViaRestore()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "file.pdf", "application/pdf", "user123");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => document.Restore("user456"));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Create_WithUnicodeCharacters_ShouldSucceed()
    {
        // Arrange & Act
        var document = DocumentEntity.Create(
            "文档标题 📄",
            "Документ описание",
            "file.pdf",
            "application/pdf",
            "user123");

        // Assert
        Assert.Equal("文档标题 📄", document.Title);
        Assert.Equal("Документ описание", document.Description);
    }

    [Fact]
    public void Create_WithSpecialCharactersInTitle_ShouldSucceed()
    {
        // Arrange & Act
        var document = DocumentEntity.Create(
            "Test & Document <with> \"special\" 'characters'",
            "Description",
            "file.pdf",
            "application/pdf",
            "user123");

        // Assert
        Assert.Contains("&", document.Title);
        Assert.Contains("<", document.Title);
        Assert.Contains(">", document.Title);
    }

    [Fact]
    public void SetFileInfo_WithVeryLargeSize_ShouldThrowException()
    {
        // Arrange
        var document = DocumentEntity.Create("Title", "Description", "test.pdf", "application/pdf", "user123");
        var veryLargeSize = long.MaxValue;

        // Act & Assert - Will throw when size exceeds 20MB
        Assert.Throws<InvalidOperationException>(() =>
            document.SetFileInfo("/path/test.pdf", veryLargeSize));
    }

    #endregion
}
