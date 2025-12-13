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
        _storageBasePath = Path.Combine(Directory.GetCurrentDirectory(), "DocumentStorage");
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

            var markerFilePath = Path.Combine(documentFolder, ".current_version");
            await File.WriteAllTextAsync(markerFilePath, versionNumber.ToString(), cancellationToken);
            
            _logger.LogInformation("Updated current version marker for document {DocumentId} to version {Version}", 
                documentId, versionNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating current version marker for document {DocumentId}", documentId);
            throw;
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

            var markerFilePath = Path.Combine(documentFolder, ".current_version");
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
}