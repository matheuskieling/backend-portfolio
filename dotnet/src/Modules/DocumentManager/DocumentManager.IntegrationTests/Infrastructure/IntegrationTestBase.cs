using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Common.IntegrationTests;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Api.Contracts.DocumentManager;
using Xunit;

namespace DocumentManager.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for DocumentManager integration tests with database-per-test isolation.
/// Each test method gets its own database, allowing full parallel execution.
/// xUnit creates a new instance per test method, so InitializeAsync runs per test.
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly string _testId = Guid.NewGuid().ToString("N")[..8];
    private string? _connectionString;
    protected DocumentManagerWebApplicationFactory Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

    public virtual async Task InitializeAsync()
    {
        // Create a unique database for this test instance (one per test method)
        _connectionString = await TestDatabaseManager.CreateDatabaseAsync($"{GetType().Name}_{_testId}");

        Factory = new DocumentManagerWebApplicationFactory(_connectionString);
        Client = Factory.CreateClient();
    }

    public virtual async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();

        // Drop the test database
        if (_connectionString != null)
        {
            await TestDatabaseManager.DropDatabaseAsync(_connectionString);
        }
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
