using Common.Contracts;
using DocumentManager.Application.DTOs;
using DocumentManager.Application.UseCases.Tags;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Api.Contracts.DocumentManager;

namespace Portfolio.Api.Controllers.DocumentManager;

/// <summary>
/// Manages tags for categorizing and organizing documents.
/// </summary>
[ApiController]
[Route("api/document-manager")]
[Tags("Document Manager - Tags")]
[Authorize]
[Produces("application/json")]
public class TagsController : ControllerBase
{
    private readonly CreateTagHandler _createTagHandler;
    private readonly GetTagsHandler _getTagsHandler;
    private readonly AddTagToDocumentHandler _addTagToDocumentHandler;
    private readonly RemoveTagFromDocumentHandler _removeTagFromDocumentHandler;

    public TagsController(
        CreateTagHandler createTagHandler,
        GetTagsHandler getTagsHandler,
        AddTagToDocumentHandler addTagToDocumentHandler,
        RemoveTagFromDocumentHandler removeTagFromDocumentHandler)
    {
        _createTagHandler = createTagHandler;
        _getTagsHandler = getTagsHandler;
        _addTagToDocumentHandler = addTagToDocumentHandler;
        _removeTagFromDocumentHandler = removeTagFromDocumentHandler;
    }

    /// <summary>
    /// Creates a new tag.
    /// </summary>
    /// <param name="request">The tag creation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created tag.</returns>
    /// <response code="201">Tag successfully created.</response>
    /// <response code="400">Invalid request data or tag name already exists.</response>
    /// <response code="401">Authentication required.</response>
    [HttpPost("tags")]
    [ProducesResponseType(typeof(ApiResponse<CreateTagResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CreateTagResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ApiResponse<CreateTagResponse>> Create(
        [FromBody] CreateTagRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTagCommand(request.Name);
        var result = await _createTagHandler.HandleAsync(command, cancellationToken);

        return ApiResponse.Created(new CreateTagResponse(result.Id, result.Name));
    }

    /// <summary>
    /// Retrieves all available tags.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all tags.</returns>
    /// <response code="200">Successfully retrieved tags.</response>
    /// <response code="401">Authentication required.</response>
    [HttpGet("tags")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TagDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ApiResponse<IReadOnlyList<TagDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _getTagsHandler.HandleAsync(cancellationToken);
        return ApiResponse.Success(result);
    }

    /// <summary>
    /// Adds a tag to a document.
    /// </summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="request">The tag to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success confirmation.</returns>
    /// <response code="204">Tag successfully added to document.</response>
    /// <response code="400">Tag already assigned to document.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Not authorized to modify this document.</response>
    /// <response code="404">Document or tag not found.</response>
    [HttpPost("documents/{documentId:guid}/tags")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<object>> AddTagToDocument(
        Guid documentId,
        [FromBody] AddTagToDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddTagToDocumentCommand(documentId, request.TagId);
        await _addTagToDocumentHandler.HandleAsync(command, cancellationToken);

        return ApiResponse.NoContent();
    }

    /// <summary>
    /// Removes a tag from a document.
    /// </summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="tagId">The tag ID to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success confirmation.</returns>
    /// <response code="204">Tag successfully removed from document.</response>
    /// <response code="400">Tag not assigned to document.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Not authorized to modify this document.</response>
    /// <response code="404">Document or tag not found.</response>
    [HttpDelete("documents/{documentId:guid}/tags/{tagId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<object>> RemoveTagFromDocument(
        Guid documentId,
        Guid tagId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveTagFromDocumentCommand(documentId, tagId);
        await _removeTagFromDocumentHandler.HandleAsync(command, cancellationToken);

        return ApiResponse.NoContent();
    }
}
