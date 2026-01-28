using Common.Domain;

namespace DocumentManager.Domain.Exceptions;

public sealed class FolderNotFoundException : DomainException
{
    private const string ErrorCode = "FOLDER_NOT_FOUND";

    public FolderNotFoundException(Guid folderId)
        : base(ErrorCode, $"Folder with ID '{folderId}' was not found.")
    {
    }
}
