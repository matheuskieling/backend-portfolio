using DocumentManager.Application.DTOs;
using DocumentManager.Application.Repositories;
using DocumentManager.Domain.Enums;

namespace DocumentManager.Application.UseCases.Documents;

public sealed record GetDocumentsQuery(
    string? TitleFilter = null,
    Guid? FolderId = null,
    Guid? OwnerId = null,
    DocumentStatus? Status = null,
    IEnumerable<Guid>? TagIds = null,
    int Page = 1,
    int PageSize = 20);

public sealed class GetDocumentsHandler
{
    private readonly IDocumentRepository _documentRepository;

    public GetDocumentsHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<PagedResult<DocumentDto>> HandleAsync(
        GetDocumentsQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var skip = (page - 1) * pageSize;

        var documents = await _documentRepository.SearchAsync(
            query.TitleFilter,
            query.FolderId,
            query.OwnerId,
            query.Status,
            query.TagIds,
            skip,
            pageSize,
            cancellationToken);

        var totalCount = await _documentRepository.CountAsync(
            query.TitleFilter,
            query.FolderId,
            query.OwnerId,
            query.Status,
            query.TagIds,
            cancellationToken);

        var dtos = documents.Select(d => new DocumentDto(
            d.Id,
            d.Title,
            d.Description,
            d.Status,
            d.CurrentVersionNumber,
            d.FolderId,
            d.OwnerId,
            d.CreatedAt,
            d.UpdatedAt)).ToList().AsReadOnly();

        return new PagedResult<DocumentDto>(dtos, totalCount, page, pageSize);
    }
}
