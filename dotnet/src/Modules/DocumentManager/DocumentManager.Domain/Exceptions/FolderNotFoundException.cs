using Common.Domain.Exceptions;

namespace DocumentManager.Domain.Exceptions;

public sealed class FolderNotFoundException : NotFoundException
{
    private const string ErrorCode = "FOLDER_NOT_FOUND";

    public FolderNotFoundException(Guid folderId)
        : base(ErrorCode, $"Folder with ID '{folderId}' was not found.")
    {
    }
}
