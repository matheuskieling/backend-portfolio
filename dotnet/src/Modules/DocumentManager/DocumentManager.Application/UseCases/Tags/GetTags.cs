using DocumentManager.Application.DTOs;
using DocumentManager.Application.Repositories;

namespace DocumentManager.Application.UseCases.Tags;

public sealed class GetTagsHandler
{
    private readonly ITagRepository _tagRepository;

    public GetTagsHandler(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<IReadOnlyList<TagDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var tags = await _tagRepository.GetAllAsync(cancellationToken);

        return tags
            .OrderBy(t => t.Name)
            .Select(t => new TagDto(t.Id, t.Name))
            .ToList()
            .AsReadOnly();
    }
}
