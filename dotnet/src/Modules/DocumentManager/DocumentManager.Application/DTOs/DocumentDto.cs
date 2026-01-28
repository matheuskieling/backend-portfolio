using DocumentManager.Domain.Enums;

namespace DocumentManager.Application.DTOs;

public sealed record DocumentDto(
    Guid Id,
    string Title,
    string? Description,
    DocumentStatus Status,
    int CurrentVersionNumber,
    Guid? FolderId,
    Guid OwnerId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record DocumentDetailDto(
    Guid Id,
    string Title,
    string? Description,
    DocumentStatus Status,
    int CurrentVersionNumber,
    Guid? FolderId,
    string? FolderName,
    Guid OwnerId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<DocumentVersionDto> Versions,
    IReadOnlyList<TagDto> Tags);

public sealed record DocumentVersionDto(
    Guid Id,
    int VersionNumber,
    string FileName,
    string MimeType,
    long FileSize,
    Guid? UploadedBy,
    DateTime UploadedAt);
