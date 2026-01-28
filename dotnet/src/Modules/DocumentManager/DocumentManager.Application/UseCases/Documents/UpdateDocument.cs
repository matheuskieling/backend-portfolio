using DocumentManager.Application.Common.Interfaces;
using DocumentManager.Application.Repositories;
using DocumentManager.Domain.Exceptions;
using Identity.Application.Common.Interfaces;

namespace DocumentManager.Application.UseCases.Documents;

public sealed record UpdateDocumentCommand(
    Guid DocumentId,
    string Title,
    string? Description);

public sealed record UpdateDocumentResult(
    Guid Id,
    string Title);

public sealed class UpdateDocumentHandler
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentManagerUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateDocumentHandler(
        IDocumentRepository documentRepository,
        IDocumentManagerUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<UpdateDocumentResult> HandleAsync(
        UpdateDocumentCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new InvalidOperationException("User must be authenticated to update a document.");

        var document = await _documentRepository.GetByIdAsync(command.DocumentId, cancellationToken)
            ?? throw new DocumentNotFoundException(command.DocumentId);

        document.EnsureCanBeModifiedBy(userId);
        document.Update(command.Title, command.Description);

        _documentRepository.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateDocumentResult(document.Id, document.Title);
    }
}
