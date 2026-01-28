using Common.Contracts;
using DocumentManager.Application.DTOs;
using DocumentManager.Application.UseCases.Folders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Api.Contracts.DocumentManager;

namespace Portfolio.Api.Controllers.DocumentManager;

/// <summary>
/// Manages folder hierarchy for organizing documents.
/// </summary>
[ApiController]
[Route("api/document-manager/folders")]
[Tags("Document Manager - Folders")]
[Authorize]
[Produces("application/json")]
public class FoldersController : ControllerBase
{
    private readonly CreateFolderHandler _createFolderHandler;
    private readonly GetFolderTreeHandler _getFolderTreeHandler;

    public FoldersController(
        CreateFolderHandler createFolderHandler,
        GetFolderTreeHandler getFolderTreeHandler)
    {
        _createFolderHandler = createFolderHandler;
        _getFolderTreeHandler = getFolderTreeHandler;
    }

    /// <summary>
    /// Creates a new folder.
    /// </summary>
    /// <param name="request">The folder creation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created folder.</returns>
    /// <response code="201">Folder successfully created.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Parent folder not found.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateFolderResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CreateFolderResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<CreateFolderResponse>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<CreateFolderResponse>> Create(
        [FromBody] CreateFolderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateFolderCommand(request.Name, request.ParentFolderId);
        var result = await _createFolderHandler.HandleAsync(command, cancellationToken);

        return ApiResponse.Created(new CreateFolderResponse(
            result.Id,
            result.Name,
            result.ParentFolderId));
    }

    /// <summary>
    /// Retrieves the complete folder hierarchy as a tree structure.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A hierarchical list of folders.</returns>
    /// <response code="200">Successfully retrieved folder tree.</response>
    /// <response code="401">Authentication required.</response>
    [HttpGet("tree")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FolderTreeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ApiResponse<IReadOnlyList<FolderTreeDto>>> GetTree(CancellationToken cancellationToken)
    {
        var result = await _getFolderTreeHandler.HandleAsync(cancellationToken);
        return ApiResponse.Success(result);
    }
}
