namespace DocumentManager.Application.DTOs;

public sealed record FolderDto(
    Guid Id,
    string Name,
    Guid? ParentFolderId,
    DateTime CreatedAt,
    Guid? CreatedBy);

public sealed record FolderTreeDto(
    Guid Id,
    string Name,
    Guid? ParentFolderId,
    DateTime CreatedAt,
    IReadOnlyList<FolderTreeDto> Children);
