using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Common.IntegrationTests;
using DocumentManager.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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

        // Clean DocumentManager data using TRUNCATE CASCADE to handle foreign keys
        var documentContext = scope.ServiceProvider.GetRequiredService<DocumentManagerDbContext>();
        await documentContext.Database.ExecuteSqlRawAsync(@"
            TRUNCATE TABLE
                dotnet_document_manager.approval_decisions,
                dotnet_document_manager.approval_requests,
                dotnet_document_manager.approval_steps,
                dotnet_document_manager.approval_workflows,
                dotnet_document_manager.document_tags,
                dotnet_document_manager.document_versions,
                dotnet_document_manager.documents,
                dotnet_document_manager.folders,
                dotnet_document_manager.tags,
                dotnet_document_manager.audit_logs
            CASCADE;
        ");

        // Clean Identity user data only - preserve seeded roles/permissions
        var identityContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await identityContext.Database.ExecuteSqlRawAsync(@"
            TRUNCATE TABLE
                dotnet_identity.user_roles,
                dotnet_identity.users
            CASCADE;
        ");
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

    // DOCUMENT_REVIEWER role ID from migration (has approval:review permission)
    private static readonly Guid DocumentReviewerRoleId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    protected async Task<string> RegisterAndLoginUserAsync(string email = "test@example.com")
    {
        var registerRequest = new
        {
            Email = email,
            Password = "SecurePassword123!",
            FirstName = "Test",
            LastName = "User"
        };
        var registerResponse = await PostAsync(Urls.Register, registerRequest);
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<RegisterResponseWrapper>();
        var userId = registerResult!.Data!.UserId;

        // Assign DOCUMENT_REVIEWER role to user (has approval:review permission)
        using var scope = Factory.Services.CreateScope();
        var identityContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await identityContext.Database.ExecuteSqlRawAsync($@"
            INSERT INTO dotnet_identity.user_roles (""Id"", ""UserId"", ""RoleId"", ""AssignedAt"")
            VALUES ('{Guid.NewGuid()}', '{userId}', '{DocumentReviewerRoleId}', '{DateTime.UtcNow:O}');
        ");

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
    private record RegisterResponseWrapper(bool Succeeded, RegisterData? Data);
    private record RegisterData(Guid UserId, string Email, string FullName);
}
