using Document.Domain.Entities;
using Document.Domain.Enums;
using Document.Infrastructure.Data;
using Document.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Document.Infrastructure.Tests.Repositories;

public class DocumentRepositoryTests : IDisposable
{
    private readonly DocumentDbContext _context;
    private readonly DocumentRepository _repository;

    public DocumentRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DocumentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new DocumentDbContext(options);
        _repository = new DocumentRepository(_context);
    }

    public void Dispose() => _context?.Dispose();

    [Fact]
    public async Task GetByIdAsync_WithExistingDocument_ShouldReturnDocument()
    {
        // Arrange
        var document = DocumentEntity.Create("Test", "Desc", "file.pdf", "application/pdf", "user1");
        await _context.Documents.AddAsync(document);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(document.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(document.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentDocument_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllActiveAsync_ShouldReturnOnlyActiveDocuments()
    {
        // Arrange
        var doc1 = DocumentEntity.Create("Doc1", "Desc", "file1.pdf", "application/pdf", "user1");
        var doc2 = DocumentEntity.Create("Doc2", "Desc", "file2.pdf", "application/pdf", "user1");
        var doc3 = DocumentEntity.Create("Doc3", "Desc", "file3.pdf", "application/pdf", "user1");
        doc2.SoftDelete("user1");
        await _context.Documents.AddRangeAsync(doc1, doc2, doc3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllActiveAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, d => d.Id == doc2.Id);
    }

    [Fact]
    public async Task AddAsync_ShouldAddDocument()
    {
        // Arrange
        var document = DocumentEntity.Create("Test", "Desc", "file.pdf", "application/pdf", "user1");

        // Act
        await _repository.AddAsync(document);
        await _context.SaveChangesAsync();

        // Assert
        var saved = await _context.Documents.FindAsync(document.Id);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task Update_ShouldUpdateDocument()
    {
        // Arrange
        var document = DocumentEntity.Create("Original", "Desc", "file.pdf", "application/pdf", "user1");
        await _context.Documents.AddAsync(document);
        await _context.SaveChangesAsync();

        // Act
        document.Update("Updated", "New Desc", "user2");
        _repository.Update(document);
        await _context.SaveChangesAsync();

        // Assert
        var updated = await _context.Documents.FindAsync(document.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated", updated.Title);
    }

    [Fact]
    public async Task CountActiveAsync_ShouldCountOnlyActiveDocuments()
    {
        // Arrange
        var doc1 = DocumentEntity.Create("Doc1", "Desc", "file1.pdf", "application/pdf", "user1");
        var doc2 = DocumentEntity.Create("Doc2", "Desc", "file2.pdf", "application/pdf", "user1");
        doc2.SoftDelete("user1");
        await _context.Documents.AddRangeAsync(doc1, doc2);
        await _context.SaveChangesAsync();

        // Act
        var count = await _repository.CountActiveAsync();

        // Assert
        Assert.Equal(1, count);
    }
}
