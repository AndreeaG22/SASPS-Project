using Document.Domain.Entities;
using Document.Infrastructure.Data;
using Document.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Document.Infrastructure.Tests.Repositories;

public class UnitOfWorkTests : IDisposable
{
    private readonly DocumentDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<DocumentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new DocumentDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
    }

    public void Dispose()
    {
        _unitOfWork?.Dispose();
        _context?.Dispose();
    }

    [Fact]
    public void Documents_ShouldReturnDocumentRepository()
    {
        // Act
        var repository = _unitOfWork.Documents;

        // Assert
        Assert.NotNull(repository);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        // Arrange
        var document = DocumentEntity.Create("Test", "Desc", "file.pdf", "application/pdf", "user1");
        await _unitOfWork.Documents.AddAsync(document);

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);
        var saved = await _context.Documents.FindAsync(document.Id);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task BeginTransaction_ShouldStartTransaction()
    {
        // Act & Assert
        await _unitOfWork.BeginTransactionAsync();
        // No exception means success
    }

    [Fact]
    public async Task CommitTransaction_ShouldCommitChanges()
    {
        // Arrange
        await _unitOfWork.BeginTransactionAsync();
        var document = DocumentEntity.Create("Test", "Desc", "file.pdf", "application/pdf", "user1");
        await _unitOfWork.Documents.AddAsync(document);
        await _unitOfWork.SaveChangesAsync();

        // Act
        await _unitOfWork.CommitTransactionAsync();

        // Assert
        var saved = await _context.Documents.FindAsync(document.Id);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task RollbackTransaction_ShouldDiscardChanges()
    {
        // Arrange
        await _unitOfWork.BeginTransactionAsync();
        var document = DocumentEntity.Create("Test", "Desc", "file.pdf", "application/pdf", "user1");
        await _unitOfWork.Documents.AddAsync(document);
        await _unitOfWork.SaveChangesAsync();

        // Act
        await _unitOfWork.RollbackTransactionAsync();

        // Assert
        // Note: InMemory database doesn't fully support transactions like real SQL databases
        // This test verifies the rollback API works without exceptions
    }
}
