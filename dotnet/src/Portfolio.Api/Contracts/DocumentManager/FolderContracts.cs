namespace Portfolio.Api.Contracts.DocumentManager;

/// <summary>
/// Request model for creating a new folder.
/// </summary>
/// <param name="Name">The folder name.</param>
/// <param name="ParentFolderId">Optional parent folder ID for creating nested folders.</param>
public sealed record CreateFolderRequest(
    string Name,
    Guid? ParentFolderId);

/// <summary>
/// Response model for folder creation.
/// </summary>
/// <param name="Id">The unique identifier of the created folder.</param>
/// <param name="Name">The folder name.</param>
/// <param name="ParentFolderId">The parent folder ID, if any.</param>
public sealed record CreateFolderResponse(
    Guid Id,
    string Name,
    Guid? ParentFolderId);
