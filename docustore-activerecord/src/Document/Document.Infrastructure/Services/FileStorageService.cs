using Document.Application.Services;
using Microsoft.Extensions.Logging;

namespace Document.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _storageBasePath;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(ILogger<FileStorageService> logger)
    {
        _logger = logger;
        _storageBasePath = Path.Combine(Directory.GetCurrentDirectory(), "DocumentStorage");
        
        // Ensure storage directory exists
        if (!Directory.Exists(_storageBasePath))
        {
            Directory.CreateDirectory(_storageBasePath);
            _logger.LogInformation("Created document storage directory at {Path}", _storageBasePath);
        }
    }

    public async Task<string> SaveFileAsync(byte[] fileContent, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate unique file name to avoid conflicts
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(_storageBasePath, uniqueFileName);

            await File.WriteAllBytesAsync(filePath, fileContent, cancellationToken);
            
            _logger.LogInformation("File saved successfully: {FilePath}", filePath);
            
            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file: {FileName}", fileName);
            throw new InvalidOperationException($"Failed to save file: {fileName}", ex);
        }
    }

    public async Task<byte[]> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            return await File.ReadAllBytesAsync(filePath, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file: {FilePath}", filePath);
            throw;
        }
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
            }
            else
            {
                _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            throw new InvalidOperationException($"Failed to delete file: {filePath}", ex);
        }
    }
}