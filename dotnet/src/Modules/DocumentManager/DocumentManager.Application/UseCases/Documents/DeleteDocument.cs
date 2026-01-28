using DocumentManager.Application.Common.Interfaces;
using DocumentManager.Application.Repositories;
using DocumentManager.Domain.Exceptions;
using Identity.Application.Common.Interfaces;

namespace DocumentManager.Application.UseCases.Documents;

public sealed record DeleteDocumentCommand(Guid DocumentId);

public sealed class DeleteDocumentHandler
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentManagerUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteDocumentHandler(
        IDocumentRepository documentRepository,
        IDocumentManagerUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(
        DeleteDocumentCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new InvalidOperationException("User must be authenticated to delete a document.");

        var document = await _documentRepository.GetByIdAsync(command.DocumentId, cancellationToken)
            ?? throw new DocumentNotFoundException(command.DocumentId);

        // Only owner or admin can delete
        if (!document.IsOwnedBy(userId) && !_currentUserService.HasPermission("document:manage_all"))
            throw new UnauthorizedDocumentAccessException(command.DocumentId, userId);

        document.SoftDelete(userId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
