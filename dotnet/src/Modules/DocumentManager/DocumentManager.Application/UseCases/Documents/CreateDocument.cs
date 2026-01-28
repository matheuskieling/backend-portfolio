using DocumentManager.Application.Common.Interfaces;
using DocumentManager.Application.Repositories;
using DocumentManager.Domain.Entities;
using DocumentManager.Domain.Exceptions;
using Identity.Application.Common.Interfaces;

namespace DocumentManager.Application.UseCases.Documents;

public sealed record CreateDocumentCommand(
    string Title,
    string? Description,
    Guid? FolderId);

public sealed record CreateDocumentResult(
    Guid Id,
    string Title);

public sealed class CreateDocumentHandler
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly IDocumentManagerUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateDocumentHandler(
        IDocumentRepository documentRepository,
        IFolderRepository folderRepository,
        IDocumentManagerUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _documentRepository = documentRepository;
        _folderRepository = folderRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<CreateDocumentResult> HandleAsync(
        CreateDocumentCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new InvalidOperationException("User must be authenticated to create a document.");

        if (command.FolderId.HasValue)
        {
            var folderExists = await _folderRepository.ExistsAsync(command.FolderId.Value, cancellationToken);
            if (!folderExists)
                throw new FolderNotFoundException(command.FolderId.Value);
        }

        var document = Document.Create(command.Title, userId, command.Description, command.FolderId);

        _documentRepository.Add(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateDocumentResult(document.Id, document.Title);
    }
}
