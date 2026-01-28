using Common.Contracts;
using DocumentManager.Application.Common.Interfaces;
using DocumentManager.Application.Repositories;
using DocumentManager.Domain.Exceptions;
using Identity.Application.Common.Interfaces;

namespace DocumentManager.Application.UseCases.Versions;

public sealed record UploadVersionCommand(
    Guid DocumentId,
    string FileName,
    string MimeType,
    long FileSize,
    Stream FileStream);

public sealed record UploadVersionResult(
    Guid VersionId,
    int VersionNumber,
    string FileName);

public sealed class UploadVersionHandler
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IDocumentManagerUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UploadVersionHandler(
        IDocumentRepository documentRepository,
        IFileStorageService fileStorageService,
        IDocumentManagerUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _documentRepository = documentRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<UploadVersionResult> HandleAsync(
        UploadVersionCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new InvalidOperationException("User must be authenticated to upload a version.");

        var document = await _documentRepository.GetByIdWithVersionsAsync(command.DocumentId, cancellationToken)
            ?? throw new DocumentNotFoundException(command.DocumentId);

        document.EnsureCanBeModifiedBy(userId);

        // Store the file
        var storagePath = await _fileStorageService.SaveFileAsync(
            command.FileStream,
            command.FileName,
            command.MimeType,
            cancellationToken);

        // Add version to document
        var version = document.AddVersion(
            command.FileName,
            command.MimeType,
            command.FileSize,
            storagePath,
            userId);

        _documentRepository.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UploadVersionResult(version.Id, version.VersionNumber, version.FileName.Value);
    }
}
