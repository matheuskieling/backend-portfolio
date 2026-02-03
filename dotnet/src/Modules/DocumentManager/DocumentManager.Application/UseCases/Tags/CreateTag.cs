using DocumentManager.Application.Common.Interfaces;
using DocumentManager.Application.Repositories;
using Common.Domain.Exceptions;
using DocumentManager.Domain.Entities;

namespace DocumentManager.Application.UseCases.Tags;

public sealed record CreateTagCommand(string Name);

public sealed record CreateTagResult(Guid Id, string Name);

public sealed class TagAlreadyExistsException : ConflictException
{
    public TagAlreadyExistsException(string name)
        : base("TAG_ALREADY_EXISTS", $"A tag with name '{name}' already exists.")
    {
    }
}

public sealed class CreateTagHandler
{
    private readonly ITagRepository _tagRepository;
    private readonly IDocumentManagerUnitOfWork _unitOfWork;

    public CreateTagHandler(
        ITagRepository tagRepository,
        IDocumentManagerUnitOfWork unitOfWork)
    {
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateTagResult> HandleAsync(
        CreateTagCommand command,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = command.Name.Trim().ToLowerInvariant();

        var exists = await _tagRepository.ExistsByNameAsync(normalizedName, cancellationToken);
        if (exists)
            throw new TagAlreadyExistsException(normalizedName);

        var tag = Tag.Create(command.Name);

        _tagRepository.Add(tag);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateTagResult(tag.Id, tag.Name);
    }
}
