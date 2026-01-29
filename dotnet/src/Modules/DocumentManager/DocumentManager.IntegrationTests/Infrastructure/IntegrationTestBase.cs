using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Common.IntegrationTests;
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

    /// <summary>
    /// The currently authenticated test user. Set by AuthenticateAsync.
    /// Use this to access UserId, assign/remove roles, or refresh the token.
    /// </summary>
    protected TestUser CurrentTestUser { get; private set; } = null!;

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

    /// <summary>
    /// Creates a test user, authenticates them, and optionally assigns additional roles.
    /// New users automatically get the MANAGER role on registration.
    /// DOCUMENT_REVIEWER role is always assigned for document manager tests.
    /// </summary>
    /// <param name="email">Optional email for the user (auto-generated if null)</param>
    /// <param name="isAdmin">If true, assigns the ADMIN role to the user</param>
    /// <param name="additionalRoles">Additional roles to assign</param>
    /// <returns>The TestUser instance for further operations</returns>
    protected async Task<TestUser> AuthenticateAsync(
        string? email = null,
        bool isAdmin = false,
        params string[] additionalRoles)
    {
        var user = await TestUser.CreateAsync(Client, email);
        user.Authenticate();

        if (isAdmin)
        {
            await user.AssignRoleAsync("ADMIN");
        }

        foreach (var role in additionalRoles)
        {
            await user.AssignRoleAsync(role);
        }

        // Refresh token to get updated roles/permissions if roles were assigned
        if (isAdmin || additionalRoles.Length > 0)
        {
            await user.RefreshTokenAsync();
        }

        CurrentTestUser = user;
        return user;
    }

    [Obsolete("Use AuthenticateAsync instead")]
    protected async Task<string> RegisterAndLoginUserAsync(string email = "test@example.com")
    {
        var user = await AuthenticateAsync(email);
        return user.GetToken();
    }

    protected void SetAuthorizationHeader(string token)
        => Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    protected void ClearAuthorizationHeader()
        => Client.DefaultRequestHeaders.Authorization = null;

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
}
