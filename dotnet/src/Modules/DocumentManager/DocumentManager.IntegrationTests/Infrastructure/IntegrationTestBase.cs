using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Common.IntegrationTests;
using DocumentManager.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Api.Contracts.DocumentManager;
using Xunit;

namespace DocumentManager.IntegrationTests.Infrastructure;

[Collection(nameof(IntegrationTestCollection))]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly DocumentManagerWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(DocumentManagerWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual async Task DisposeAsync()
    {
        using var scope = Factory.Services.CreateScope();

        // Clean DocumentManager data
        var documentContext = scope.ServiceProvider.GetRequiredService<DocumentManagerDbContext>();
        documentContext.ApprovalDecisions.RemoveRange(documentContext.ApprovalDecisions);
        documentContext.ApprovalRequests.RemoveRange(documentContext.ApprovalRequests);
        documentContext.ApprovalSteps.RemoveRange(documentContext.ApprovalSteps);
        documentContext.ApprovalWorkflows.RemoveRange(documentContext.ApprovalWorkflows);
        documentContext.DocumentTags.RemoveRange(documentContext.DocumentTags);
        documentContext.DocumentVersions.RemoveRange(documentContext.DocumentVersions);
        documentContext.Documents.RemoveRange(documentContext.Documents);
        documentContext.Folders.RemoveRange(documentContext.Folders);
        documentContext.Tags.RemoveRange(documentContext.Tags);
        documentContext.AuditLogs.RemoveRange(documentContext.AuditLogs);
        await documentContext.SaveChangesAsync();

        // Clean Identity data
        var identityContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        identityContext.Users.RemoveRange(identityContext.Users);
        identityContext.Roles.RemoveRange(identityContext.Roles);
        identityContext.Permissions.RemoveRange(identityContext.Permissions);
        await identityContext.SaveChangesAsync();
    }

    #region HTTP Helpers

    protected Task<HttpResponseMessage> PostAsync<T>(string url, T content)
        => Client.PostAsJsonAsync(url, content);

    protected Task<HttpResponseMessage> GetAsync(string url)
        => Client.GetAsync(url);

    protected Task<HttpResponseMessage> PutAsync<T>(string url, T content)
        => Client.PutAsJsonAsync(url, content);

    protected Task<HttpResponseMessage> DeleteAsync(string url)
        => Client.DeleteAsync(url);

    protected Task<HttpResponseMessage> PostMultipartAsync(string url, MultipartFormDataContent content)
        => Client.PostAsync(url, content);

    #endregion

    #region Auth Helpers

    protected async Task<string> RegisterAndLoginUserAsync(string email = "test@example.com")
    {
        var registerRequest = new
        {
            Email = email,
            Password = "SecurePassword123!",
            FirstName = "Test",
            LastName = "User"
        };
        await PostAsync(Urls.Register, registerRequest);

        var loginRequest = new { Email = email, Password = "SecurePassword123!" };
        var response = await PostAsync(Urls.Login, loginRequest);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseWrapper>();

        return loginResponse!.Data!.Token;
    }

    protected void SetAuthorizationHeader(string token)
        => Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    protected void ClearAuthorizationHeader()
        => Client.DefaultRequestHeaders.Authorization = null;

    protected async Task AuthenticateAsync(string email = "test@example.com")
    {
        var token = await RegisterAndLoginUserAsync(email);
        SetAuthorizationHeader(token);
    }

    #endregion

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

    private record LoginResponseWrapper(bool Succeeded, LoginData? Data);
    private record LoginData(string Token, Guid UserId, string Email, string FullName);
}
