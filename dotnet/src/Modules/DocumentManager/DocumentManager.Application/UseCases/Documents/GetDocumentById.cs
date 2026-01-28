using DocumentManager.Application.DTOs;
using DocumentManager.Application.Repositories;
using DocumentManager.Domain.Exceptions;

namespace DocumentManager.Application.UseCases.Documents;

public sealed record GetDocumentByIdQuery(Guid DocumentId);

public sealed class GetDocumentByIdHandler
{
    private readonly IDocumentRepository _documentRepository;

    public GetDocumentByIdHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<DocumentDetailDto> HandleAsync(
        GetDocumentByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdWithAllAsync(query.DocumentId, cancellationToken)
            ?? throw new DocumentNotFoundException(query.DocumentId);

        var versions = document.Versions
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new DocumentVersionDto(
                v.Id,
                v.VersionNumber,
                v.FileName.Value,
                v.MimeType.Value,
                v.FileSize,
                v.UploadedBy,
                v.UploadedAt))
            .ToList()
            .AsReadOnly();

        var tags = document.DocumentTags
            .Select(dt => new TagDto(dt.Tag.Id, dt.Tag.Name))
            .ToList()
            .AsReadOnly();

        return new DocumentDetailDto(
            document.Id,
            document.Title,
            document.Description,
            document.Status,
            document.CurrentVersionNumber,
            document.FolderId,
            document.Folder?.Name,
            document.OwnerId,
            document.CreatedAt,
            document.UpdatedAt,
            versions,
            tags);
    }
}
