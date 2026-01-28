using Common.Contracts;

namespace Common.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public FileStorageService(string basePath)
    {
        _basePath = basePath;
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string mimeType, CancellationToken cancellationToken = default)
    {
        var uniqueId = Guid.NewGuid().ToString("N");
        var extension = Path.GetExtension(fileName);
        var storedFileName = $"{uniqueId}{extension}";

        // Create subdirectory based on date for organization
        var dateFolder = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var fullDirectoryPath = Path.Combine(_basePath, dateFolder);

        if (!Directory.Exists(fullDirectoryPath))
        {
            Directory.CreateDirectory(fullDirectoryPath);
        }

        var fullPath = Path.Combine(fullDirectoryPath, storedFileName);
        var relativePath = Path.Combine(dateFolder, storedFileName);

        await using var fileStreamOutput = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
        await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);

        return relativePath;
    }

    public async Task<Stream?> GetFileAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, storagePath);

        if (!File.Exists(fullPath))
        {
            return null;
        }

        var memoryStream = new MemoryStream();
        await using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public Task DeleteFileAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, storagePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, storagePath);
        return Task.FromResult(File.Exists(fullPath));
    }
}
