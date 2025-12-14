using Versioning.Domain.Entities;

namespace Versioning.Domain.Tests.Entities;

public class VersionEntityTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateVersion()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var versionNumber = 1;
        var fileName = "test.pdf";
        var contentType = "application/pdf";
        var notes = "Initial version";
        var createdBy = "user123";

        // Act
        var version = VersionEntity.Create(documentId, versionNumber, fileName, contentType, notes, createdBy);

        // Assert
        Assert.NotNull(version);
        Assert.Equal(documentId, version.DocumentId);
        Assert.Equal(versionNumber, version.VersionNumber);
        Assert.Equal(fileName, version.FileName);
        Assert.Equal(contentType, version.ContentType);
        Assert.Equal(notes, version.Notes);
        Assert.True(version.IsCurrent);
        Assert.Equal(createdBy, version.CreatedBy);
    }

    [Fact]
    public void Create_WithEmptyDocumentId_ShouldThrowException()
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            VersionEntity.Create(Guid.Empty, 1, "file.pdf", "application/pdf", null, "user123"));
        Assert.Contains("Document ID is required", exception.Message);
    }

    [Fact]
    public void Create_WithEmptyFileName_ShouldThrowException()
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            VersionEntity.Create(Guid.NewGuid(), 1, "", "application/pdf", null, "user123"));
        Assert.Contains("File name is required", exception.Message);
    }

    [Fact]
    public void Create_WithLongNotes_ShouldThrowException()
    {
        // Arrange
        var longNotes = new string('a', 501);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            VersionEntity.Create(Guid.NewGuid(), 1, "file.pdf", "application/pdf", longNotes, "user123"));
        Assert.Contains("Notes cannot exceed 500 characters", exception.Message);
    }

    [Fact]
    public void SetFileInfo_WithValidData_ShouldSetProperties()
    {
        // Arrange
        var version = VersionEntity.Create(Guid.NewGuid(), 1, "test.pdf", "application/pdf", null, "user123");
        var filePath = "/uploads/test.pdf";
        var fileSize = 1024L;

        // Act
        version.SetFileInfo(filePath, fileSize);

        // Assert
        Assert.Equal(filePath, version.FilePathOnDisk);
        Assert.Equal(fileSize, version.FileSizeInBytes);
    }

    [Fact]
    public void SetFileInfo_WithOversizedFile_ShouldThrowException()
    {
        // Arrange
        var version = VersionEntity.Create(Guid.NewGuid(), 1, "test.pdf", "application/pdf", null, "user123");
        var oversizedFile = (20 * 1024 * 1024) + 1;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            version.SetFileInfo("/path/file.pdf", oversizedFile));
        Assert.Contains("File size exceeds the maximum allowed size", exception.Message);
    }

    [Fact]
    public void SetAsCurrent_ShouldUpdateProperties()
    {
        // Arrange
        var version = VersionEntity.Create(Guid.NewGuid(), 1, "test.pdf", "application/pdf", null, "user123");
        version.MarkAsNotCurrent();

        // Act
        version.SetAsCurrent("user456");

        // Assert
        Assert.True(version.IsCurrent);
        Assert.Equal("user456", version.UpdatedBy);
        Assert.NotNull(version.UpdatedAt);
    }

    [Fact]
    public void MarkAsNotCurrent_ShouldSetIsCurrentToFalse()
    {
        // Arrange
        var version = VersionEntity.Create(Guid.NewGuid(), 1, "test.pdf", "application/pdf", null, "user123");

        // Act
        version.MarkAsNotCurrent();

        // Assert
        Assert.False(version.IsCurrent);
    }
}
