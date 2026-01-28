namespace Portfolio.Api.Contracts.DocumentManager;

/// <summary>
/// Request model for creating a new tag.
/// </summary>
/// <param name="Name">The tag name (will be normalized to lowercase).</param>
public sealed record CreateTagRequest(string Name);

/// <summary>
/// Request model for adding a tag to a document.
/// </summary>
/// <param name="TagId">The ID of the tag to add.</param>
public sealed record AddTagToDocumentRequest(Guid TagId);

/// <summary>
/// Response model for tag creation.
/// </summary>
/// <param name="Id">The unique identifier of the created tag.</param>
/// <param name="Name">The normalized tag name.</param>
public sealed record CreateTagResponse(Guid Id, string Name);
