using DocumentManager.Application.DTOs;
using DocumentManager.Application.Repositories;

namespace DocumentManager.Application.UseCases.Folders;

public sealed class GetFolderTreeHandler
{
    private readonly IFolderRepository _folderRepository;

    public GetFolderTreeHandler(IFolderRepository folderRepository)
    {
        _folderRepository = folderRepository;
    }

    public async Task<IReadOnlyList<FolderTreeDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var folders = await _folderRepository.GetAllWithHierarchyAsync(cancellationToken);

        var folderLookup = folders.ToDictionary(f => f.Id);
        var rootFolders = folders.Where(f => f.ParentFolderId == null);

        return rootFolders.Select(f => BuildTree(f, folderLookup)).ToList().AsReadOnly();
    }

    private static FolderTreeDto BuildTree(
        Domain.Entities.Folder folder,
        Dictionary<Guid, Domain.Entities.Folder> folderLookup)
    {
        var children = folderLookup.Values
            .Where(f => f.ParentFolderId == folder.Id)
            .Select(f => BuildTree(f, folderLookup))
            .OrderBy(f => f.Name)
            .ToList()
            .AsReadOnly();

        return new FolderTreeDto(
            folder.Id,
            folder.Name,
            folder.ParentFolderId,
            folder.CreatedAt,
            children);
    }
}
