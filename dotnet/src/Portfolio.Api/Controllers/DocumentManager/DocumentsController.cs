using Common.Contracts;
using DocumentManager.Application.DTOs;
using DocumentManager.Application.UseCases.Documents;
using DocumentManager.Application.UseCases.Versions;
using DocumentManager.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Api.Contracts.DocumentManager;

namespace Portfolio.Api.Controllers.DocumentManager;

/// <summary>
/// Manages documents including CRUD operations, versioning, and history.
/// </summary>
[ApiController]
[Route("api/document-manager/documents")]
[Tags("Document Manager - Documents")]
[Authorize]
[Produces("application/json")]
public class DocumentsController : ControllerBase
{
    private readonly CreateDocumentHandler _createDocumentHandler;
    private readonly GetDocumentsHandler _getDocumentsHandler;
    private readonly GetDocumentByIdHandler _getDocumentByIdHandler;
    private readonly UpdateDocumentHandler _updateDocumentHandler;
    private readonly DeleteDocumentHandler _deleteDocumentHandler;
    private readonly GetDocumentHistoryHandler _getDocumentHistoryHandler;
    private readonly UploadVersionHandler _uploadVersionHandler;

    public DocumentsController(
        CreateDocumentHandler createDocumentHandler,
        GetDocumentsHandler getDocumentsHandler,
        GetDocumentByIdHandler getDocumentByIdHandler,
        UpdateDocumentHandler updateDocumentHandler,
        DeleteDocumentHandler deleteDocumentHandler,
        GetDocumentHistoryHandler getDocumentHistoryHandler,
        UploadVersionHandler uploadVersionHandler)
    {
        _createDocumentHandler = createDocumentHandler;
        _getDocumentsHandler = getDocumentsHandler;
        _getDocumentByIdHandler = getDocumentByIdHandler;
        _updateDocumentHandler = updateDocumentHandler;
        _deleteDocumentHandler = deleteDocumentHandler;
        _getDocumentHistoryHandler = getDocumentHistoryHandler;
        _uploadVersionHandler = uploadVersionHandler;
    }

    /// <summary>
    /// Creates a new document.
    /// </summary>
    /// <param name="request">The document creation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created document.</returns>
    /// <response code="201">Document successfully created.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Folder not found.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateDocumentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CreateDocumentResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<CreateDocumentResponse>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<CreateDocumentResponse>> Create(
        [FromBody] CreateDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateDocumentCommand(request.Title, request.Description, request.FolderId);
        var result = await _createDocumentHandler.HandleAsync(command, cancellationToken);

        return ApiResponse.Created(new CreateDocumentResponse(result.Id, result.Title));
    }

    /// <summary>
    /// Retrieves a paginated list of documents with optional filtering.
    /// </summary>
    /// <param name="title">Filter by document title (partial match).</param>
    /// <param name="folderId">Filter by folder ID.</param>
    /// <param name="ownerId">Filter by owner user ID.</param>
    /// <param name="status">Filter by document status (Draft, PendingApproval, Approved, Rejected).</param>
    /// <param name="tagIds">Filter by tag IDs (documents must have all specified tags).</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Number of items per page (default: 20, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of documents.</returns>
    /// <response code="200">Successfully retrieved documents.</response>
    /// <response code="401">Authentication required.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DocumentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ApiResponse<PagedResult<DocumentDto>>> GetAll(
        [FromQuery] string? title,
        [FromQuery] Guid? folderId,
        [FromQuery] Guid? ownerId,
        [FromQuery] DocumentStatus? status,
        [FromQuery] List<Guid>? tagIds,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDocumentsQuery(title, folderId, ownerId, status, tagIds, page, pageSize);
        var result = await _getDocumentsHandler.HandleAsync(query, cancellationToken);

        return ApiResponse.Success(result);
    }

    /// <summary>
    /// Retrieves a document by its ID with full details including versions and tags.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The document details.</returns>
    /// <response code="200">Successfully retrieved document.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Document not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DocumentDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<DocumentDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<DocumentDetailDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetDocumentByIdQuery(id);
        var result = await _getDocumentByIdHandler.HandleAsync(query, cancellationToken);

        return ApiResponse.Success(result);
    }

    /// <summary>
    /// Updates an existing document. Only draft documents can be updated.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <param name="request">The update details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated document.</returns>
    /// <response code="200">Document successfully updated.</response>
    /// <response code="400">Invalid request data or document is not in draft status.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Not authorized to update this document.</response>
    /// <response code="404">Document not found.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UpdateDocumentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UpdateDocumentResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<UpdateDocumentResponse>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<UpdateDocumentResponse>> Update(
        Guid id,
        [FromBody] UpdateDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDocumentCommand(id, request.Title, request.Description);
        var result = await _updateDocumentHandler.HandleAsync(command, cancellationToken);

        return ApiResponse.Success(new UpdateDocumentResponse(result.Id, result.Title));
    }

    /// <summary>
    /// Soft deletes a document. Only draft documents owned by the current user can be deleted.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Document successfully deleted.</response>
    /// <response code="400">Document is not in draft status.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Not authorized to delete this document.</response>
    /// <response code="404">Document not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<object>> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteDocumentCommand(id);
        await _deleteDocumentHandler.HandleAsync(command, cancellationToken);

        return ApiResponse.NoContent();
    }

    /// <summary>
    /// Retrieves the audit history for a document.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of audit log entries.</returns>
    /// <response code="200">Successfully retrieved audit history.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Document not found.</response>
    [HttpGet("{id:guid}/history")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AuditLogDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AuditLogDto>>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<IReadOnlyList<AuditLogDto>>> GetHistory(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetDocumentHistoryQuery(id);
        var result = await _getDocumentHistoryHandler.HandleAsync(query, cancellationToken);

        return ApiResponse.Success(result);
    }

    /// <summary>
    /// Uploads a new version of the document. Only draft documents can have new versions uploaded.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <param name="file">The file to upload (max 100MB).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created version details.</returns>
    /// <response code="201">Version successfully uploaded.</response>
    /// <response code="400">Invalid file or document is not in draft status.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Not authorized to upload to this document.</response>
    /// <response code="404">Document not found.</response>
    [HttpPost("{id:guid}/versions")]
    [RequestSizeLimit(100_000_000)] // 100MB limit
    [ProducesResponseType(typeof(ApiResponse<UploadVersionResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<UploadVersionResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<UploadVersionResponse>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<UploadVersionResponse>> UploadVersion(
        Guid id,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var command = new UploadVersionCommand(
            id,
            file.FileName,
            file.ContentType,
            file.Length,
            stream);

        var result = await _uploadVersionHandler.HandleAsync(command, cancellationToken);

        return ApiResponse.Created(new UploadVersionResponse(
            result.VersionId,
            result.VersionNumber,
            result.FileName));
    }
}
