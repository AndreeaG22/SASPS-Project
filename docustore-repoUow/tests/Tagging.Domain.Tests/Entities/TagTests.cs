using Tagging.Domain.Entities;

namespace Tagging.Domain.Tests.Entities;

public class TagTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateTag()
    {
        // Arrange
        var name = "Important";
        var description = "Important documents";
        var createdBy = "user123";

        // Act
        var tag = Tag.Create(name, description, createdBy);

        // Assert
        Assert.NotNull(tag);
        Assert.Equal(name, tag.Name);
        Assert.Equal(description, tag.Description);
        Assert.Equal(createdBy, tag.CreatedBy);
    }

    [Fact]
    public void Create_WithWhitespaceInName_ShouldTrimName()
    {
        // Act
        var tag = Tag.Create("  Important  ", "Description", "user123");

        // Assert
        Assert.Equal("Important", tag.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ShouldThrowException(string invalidName)
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            Tag.Create(invalidName, "Description", "user123"));
        Assert.Contains("Tag name cannot be empty", exception.Message);
    }

    [Fact]
    public void Create_WithLongName_ShouldThrowException()
    {
        // Arrange
        var longName = new string('a', 51);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            Tag.Create(longName, "Description", "user123"));
        Assert.Contains("Tag name cannot exceed 50 characters", exception.Message);
    }

    [Fact]
    public void Create_WithLongDescription_ShouldThrowException()
    {
        // Arrange
        var longDescription = new string('a', 201);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            Tag.Create("TagName", longDescription, "user123"));
        Assert.Contains("Tag description cannot exceed 200 characters", exception.Message);
    }

    [Fact]
    public void Create_WithNullDescription_ShouldSucceed()
    {
        // Act
        var tag = Tag.Create("TagName", null, "user123");

        // Assert
        Assert.Null(tag.Description);
    }

    [Fact]
    public void Create_WithWhitespaceDescription_ShouldSetToNull()
    {
        // Act
        var tag = Tag.Create("TagName", "   ", "user123");

        // Assert
        Assert.Null(tag.Description);
    }
}
