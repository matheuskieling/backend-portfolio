namespace Portfolio.Api.Contracts.DocumentManager;

/// <summary>
/// Request model for creating a new document.
/// </summary>
/// <param name="Title">The document title.</param>
/// <param name="Description">Optional description of the document.</param>
/// <param name="FolderId">Optional folder ID to place the document in.</param>
public sealed record CreateDocumentRequest(
    string Title,
    string? Description,
    Guid? FolderId);

/// <summary>
/// Request model for updating an existing document.
/// </summary>
/// <param name="Title">The new document title.</param>
/// <param name="Description">The new document description.</param>
public sealed record UpdateDocumentRequest(
    string Title,
    string? Description);

/// <summary>
/// Response model for document creation.
/// </summary>
/// <param name="Id">The unique identifier of the created document.</param>
/// <param name="Title">The document title.</param>
public sealed record CreateDocumentResponse(
    Guid Id,
    string Title);

/// <summary>
/// Response model for document update.
/// </summary>
/// <param name="Id">The document ID.</param>
/// <param name="Title">The updated document title.</param>
public sealed record UpdateDocumentResponse(
    Guid Id,
    string Title);

/// <summary>
/// Response model for version upload.
/// </summary>
/// <param name="VersionId">The unique identifier of the created version.</param>
/// <param name="VersionNumber">The sequential version number.</param>
/// <param name="FileName">The original file name.</param>
public sealed record UploadVersionResponse(
    Guid VersionId,
    int VersionNumber,
    string FileName);
