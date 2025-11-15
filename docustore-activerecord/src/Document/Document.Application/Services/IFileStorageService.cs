namespace Document.Application.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(byte[] fileContent, string fileName, CancellationToken cancellationToken = default);
    Task<byte[]> GetFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
}