using DocumentManager.Application.Common.Interfaces;
using DocumentManager.Application.Repositories;
using DocumentManager.Domain.Entities;
using DocumentManager.Domain.Exceptions;
using Identity.Application.Common.Interfaces;

namespace DocumentManager.Application.UseCases.Folders;

public sealed record CreateFolderCommand(
    string Name,
    Guid? ParentFolderId);

public sealed record CreateFolderResult(
    Guid Id,
    string Name,
    Guid? ParentFolderId);

public sealed class CreateFolderHandler
{
    private readonly IFolderRepository _folderRepository;
    private readonly IDocumentManagerUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateFolderHandler(
        IFolderRepository folderRepository,
        IDocumentManagerUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _folderRepository = folderRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<CreateFolderResult> HandleAsync(
        CreateFolderCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ParentFolderId.HasValue)
        {
            var parentExists = await _folderRepository.ExistsAsync(command.ParentFolderId.Value, cancellationToken);
            if (!parentExists)
                throw new FolderNotFoundException(command.ParentFolderId.Value);
        }

        var folder = Folder.Create(command.Name, command.ParentFolderId);

        _folderRepository.Add(folder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateFolderResult(folder.Id, folder.Name, folder.ParentFolderId);
    }
}
