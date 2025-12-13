using Document.Domain.Services;
using Microsoft.Extensions.Logging;

namespace Document.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _storageBasePath;
    private readonly ILogger<FileStorageService> _logger;
    private const string CurrentVersionFileName = ".current_version";

    public FileStorageService(ILogger<FileStorageService> logger)
    {
        _logger = logger;
        _storageBasePath = Path.Combine(Directory.GetCurrentDirectory(), "DocumentStorage");
        
        if (!Directory.Exists(_storageBasePath))
        {
            Directory.CreateDirectory(_storageBasePath);
            _logger.LogInformation("Created document storage directory at {Path}", _storageBasePath);
        }
    }

    public Task<string> CreateDocumentFolderAsync(Guid documentId, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create folder: {documentId}_{sanitizedFileName}
            var sanitizedFileName = SanitizeFileName(fileName);
            var folderName = $"{documentId}_{sanitizedFileName}";
            var folderPath = Path.Combine(_storageBasePath, folderName);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                _logger.LogInformation("Created document folder: {FolderPath}", folderPath);
            }

            return Task.FromResult(folderPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document folder for {DocumentId}", documentId);
            throw new InvalidOperationException($"Failed to create document folder for {documentId}", ex);
        }
    }

    public async Task<string> SaveVersionFileAsync(
        Guid documentId, 
        byte[] fileContent, 
        int versionNumber, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var documentFolder = GetDocumentFolder(documentId);
            if (documentFolder == null)
            {
                throw new InvalidOperationException($"Document folder not found for {documentId}");
            }

            // File name: v{versionNumber}_{originalFileName}
            var originalFileName = Path.GetFileName(documentFolder).Split('_', 2)[1];
            var versionFileName = $"v{versionNumber}_{originalFileName}";
            var versionFilePath = Path.Combine(documentFolder, versionFileName);

            await File.WriteAllBytesAsync(versionFilePath, fileContent, cancellationToken);
            _logger.LogInformation("Version {Version} saved: {FilePath}", versionNumber, versionFilePath);

            return versionFilePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving version {Version} for document {DocumentId}", versionNumber, documentId);
            throw new InvalidOperationException($"Failed to save version {versionNumber}", ex);
        }
    }

    public async Task<byte[]> GetVersionFileAsync(
        Guid documentId, 
        int versionNumber, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var documentFolder = GetDocumentFolder(documentId);
            if (documentFolder == null)
            {
                throw new FileNotFoundException($"Document folder not found for {documentId}");
            }

            var versionFiles = Directory.GetFiles(documentFolder, $"v{versionNumber}_*");
            if (versionFiles.Length == 0)
            {
                throw new FileNotFoundException($"Version {versionNumber} not found for document {documentId}");
            }

            return await File.ReadAllBytesAsync(versionFiles[0], cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading version {Version} for document {DocumentId}", versionNumber, documentId);
            throw;
        }
    }

    public async Task<byte[]> GetCurrentVersionFileAsync(
        Guid documentId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentVersion = await GetCurrentVersionNumberAsync(documentId, cancellationToken);
            if (currentVersion == null)
            {
                throw new InvalidOperationException($"No current version set for document {documentId}");
            }

            return await GetVersionFileAsync(documentId, currentVersion.Value, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading current version for document {DocumentId}", documentId);
            throw;
        }
    }

    public Task DeleteDocumentFolderAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var documentFolder = GetDocumentFolder(documentId);
            if (documentFolder != null && Directory.Exists(documentFolder))
            {
                Directory.Delete(documentFolder, recursive: true);
                _logger.LogInformation("Document folder deleted: {FolderPath}", documentFolder);
            }
            else
            {
                _logger.LogWarning("Document folder not found for deletion: {DocumentId}", documentId);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document folder for {DocumentId}", documentId);
            throw new InvalidOperationException($"Failed to delete document folder for {documentId}", ex);
        }
    }

    public async Task UpdateCurrentVersionMarkerAsync(
        Guid documentId, 
        int versionNumber, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var documentFolder = GetDocumentFolder(documentId);
            if (documentFolder == null)
            {
                throw new InvalidOperationException($"Document folder not found for {documentId}");
            }

            var markerFilePath = Path.Combine(documentFolder, CurrentVersionFileName);
            await File.WriteAllTextAsync(markerFilePath, versionNumber.ToString(), cancellationToken);
            
            _logger.LogInformation("Updated current version marker for document {DocumentId} to version {Version}", 
                documentId, versionNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating current version marker for document {DocumentId}", documentId);
            throw new InvalidOperationException($"Failed to update current version marker", ex);
        }
    }

    public async Task<int?> GetCurrentVersionNumberAsync(
        Guid documentId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var documentFolder = GetDocumentFolder(documentId);
            if (documentFolder == null)
            {
                return null;
            }

            var markerFilePath = Path.Combine(documentFolder, CurrentVersionFileName);
            if (!File.Exists(markerFilePath))
            {
                return null;
            }

            var content = await File.ReadAllTextAsync(markerFilePath, cancellationToken);
            return int.TryParse(content, out var version) ? version : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading current version marker for document {DocumentId}", documentId);
            return null;
        }
    }

    private string? GetDocumentFolder(Guid documentId)
    {
        var folders = Directory.GetDirectories(_storageBasePath, $"{documentId}_*");
        return folders.Length > 0 ? folders[0] : null;
    }

    private string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Length > 50 ? sanitized.Substring(0, 50) : sanitized;
    }
}