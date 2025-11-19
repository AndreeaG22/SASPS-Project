using Microsoft.Extensions.Logging;
using Versioning.Domain.Services;

namespace Versioning.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _storageBasePath;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(ILogger<FileStorageService> logger)
    {
        _logger = logger;
        _storageBasePath = Path.Combine(Directory.GetCurrentDirectory(), "VersionStorage");
        
        if (!Directory.Exists(_storageBasePath))
        {
            Directory.CreateDirectory(_storageBasePath);
            _logger.LogInformation("Created version storage directory at {Path}", _storageBasePath);
        }
    }

    public async Task<string> SaveFileAsync(byte[] fileContent, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(_storageBasePath, uniqueFileName);

            await File.WriteAllBytesAsync(filePath, fileContent, cancellationToken);
            
            _logger.LogInformation("Version file saved successfully: {FilePath}", filePath);
            
            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving version file: {FileName}", fileName);
            throw new InvalidOperationException($"Failed to save version file: {fileName}", ex);
        }
    }

    public async Task<byte[]> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Version file not found: {filePath}");

            return await File.ReadAllBytesAsync(filePath, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading version file: {FilePath}", filePath);
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
                _logger.LogInformation("Version file deleted successfully: {FilePath}", filePath);
            }
            else
            {
                _logger.LogWarning("Version file not found for deletion: {FilePath}", filePath);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting version file: {FilePath}", filePath);
            throw new InvalidOperationException($"Failed to delete version file: {filePath}", ex);
        }
    }
}