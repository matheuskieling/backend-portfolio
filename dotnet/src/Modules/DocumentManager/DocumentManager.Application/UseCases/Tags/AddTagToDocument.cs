using DocumentManager.Application.Common.Interfaces;
using DocumentManager.Application.Repositories;
using DocumentManager.Domain.Exceptions;
using Identity.Application.Common.Interfaces;

namespace DocumentManager.Application.UseCases.Tags;

public sealed record AddTagToDocumentCommand(
    Guid DocumentId,
    Guid TagId);

public sealed class AddTagToDocumentHandler
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IDocumentManagerUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AddTagToDocumentHandler(
        IDocumentRepository documentRepository,
        ITagRepository tagRepository,
        IDocumentManagerUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _documentRepository = documentRepository;
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(
        AddTagToDocumentCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new InvalidOperationException("User must be authenticated to add a tag to a document.");

        var document = await _documentRepository.GetByIdWithTagsAsync(command.DocumentId, cancellationToken)
            ?? throw new DocumentNotFoundException(command.DocumentId);

        var tag = await _tagRepository.GetByIdAsync(command.TagId, cancellationToken)
            ?? throw new TagNotFoundException(command.TagId);

        // Only owner or admin can modify tags
        if (!document.IsOwnedBy(userId) && !_currentUserService.HasPermission("document:manage_all"))
            throw new UnauthorizedDocumentAccessException(command.DocumentId, userId);

        document.AddTag(tag);

        _documentRepository.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
