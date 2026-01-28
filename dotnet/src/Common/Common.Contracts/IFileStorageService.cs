namespace Common.Contracts;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string mimeType, CancellationToken cancellationToken = default);
    Task<Stream?> GetFileAsync(string storagePath, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string storagePath, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string storagePath, CancellationToken cancellationToken = default);
}
