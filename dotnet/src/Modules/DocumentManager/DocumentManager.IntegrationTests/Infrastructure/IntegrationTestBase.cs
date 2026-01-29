using System.Net;
using System.Net.Http.Headers;
using Common.IntegrationTests;
using Portfolio.Api.Contracts.DocumentManager;

namespace DocumentManager.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for DocumentManager integration tests.
/// Inherits common functionality from IntegrationTestBase&lt;TFactory&gt;.
/// </summary>
public abstract class IntegrationTestBase : IntegrationTestBase<DocumentManagerWebApplicationFactory>
{
    protected override DocumentManagerWebApplicationFactory CreateFactory(string connectionString)
        => new(connectionString);

    #region Entity Creation Helpers

    protected async Task<CreateFolderResponse> CreateFolderAsync(string name = "Test Folder", Guid? parentId = null)
    {
        var response = await PostAsync(Urls.Folders, new { Name = name, ParentFolderId = parentId });
        return (await response.ValidateSuccessAsync<CreateFolderResponse>(HttpStatusCode.Created)).Data!;
    }

    protected async Task<CreateDocumentResponse> CreateDocumentAsync(string title = "Test Document", Guid? folderId = null)
    {
        var response = await PostAsync(Urls.Documents, new { Title = title, FolderId = folderId });
        return (await response.ValidateSuccessAsync<CreateDocumentResponse>(HttpStatusCode.Created)).Data!;
    }

    protected async Task<CreateDocumentResponse> CreateDocumentWithVersionAsync(string title = "Test Document")
    {
        var document = await CreateDocumentAsync(title);
        await UploadVersionAsync(document.Id);
        return document;
    }

    protected async Task<UploadVersionResponse> UploadVersionAsync(Guid documentId, string fileName = "document.pdf")
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // PDF magic bytes
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", fileName);

        var response = await PostMultipartAsync(Urls.DocumentVersions(documentId), content);
        return (await response.ValidateSuccessAsync<UploadVersionResponse>(HttpStatusCode.Created)).Data!;
    }

    protected async Task<CreateTagResponse> CreateTagAsync(string name = "test-tag")
    {
        var response = await PostAsync(Urls.Tags, new { Name = name });
        return (await response.ValidateSuccessAsync<CreateTagResponse>(HttpStatusCode.Created)).Data!;
    }

    protected async Task<CreateWorkflowResponse> CreateWorkflowAsync(
        string name = "Test Workflow",
        params (int order, string role)[] steps)
    {
        var stepsList = steps.Length > 0
            ? steps.Select(s => new { StepOrder = s.order, RequiredRole = s.role }).ToArray()
            : new[] { new { StepOrder = 1, RequiredRole = "REVIEWER" } };

        var response = await PostAsync(Urls.Workflows, new { Name = name, Steps = stepsList });
        return (await response.ValidateSuccessAsync<CreateWorkflowResponse>(HttpStatusCode.Created)).Data!;
    }

    protected async Task<SubmitForApprovalResponse> SubmitForApprovalAsync(Guid documentId, Guid workflowId)
    {
        var response = await PostAsync(Urls.SubmitDocument(documentId), new { WorkflowId = workflowId });
        return (await response.ValidateSuccessAsync<SubmitForApprovalResponse>(HttpStatusCode.Created)).Data!;
    }

    protected async Task AddTagToDocumentAsync(Guid documentId, Guid tagId)
    {
        await PostAsync(Urls.DocumentTags(documentId), new { TagId = tagId });
    }

    #endregion
}
