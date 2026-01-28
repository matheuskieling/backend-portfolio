using System.Net;
using Common.IntegrationTests;
using DocumentManager.IntegrationTests.Infrastructure;
using Portfolio.Api.Contracts.DocumentManager;
using Xunit;

namespace DocumentManager.IntegrationTests;

public class DocumentsEndpointsTests : IntegrationTestBase
{
    public DocumentsEndpointsTests(DocumentManagerWebApplicationFactory factory) : base(factory)
    {
    }

    #region Create Document

    [Fact]
    public async Task CreateDocument_WithValidData_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await PostAsync(Urls.Documents, new { Title = "Test Document", Description = "A test" });

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<CreateDocumentResponse>(HttpStatusCode.Created);
        Assert.NotEqual(Guid.Empty, apiResponse.Data!.Id);
        Assert.Equal("Test Document", apiResponse.Data.Title);
    }

    [Fact]
    public async Task CreateDocument_WithFolder_ReturnsCreatedInFolder()
    {
        // Arrange
        await AuthenticateAsync();
        var folder = await CreateFolderAsync("Documents Folder");

        // Act
        var response = await PostAsync(Urls.Documents, new { Title = "Document in Folder", FolderId = folder.Id });

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<CreateDocumentResponse>(HttpStatusCode.Created);
        Assert.Equal("Document in Folder", apiResponse.Data!.Title);
    }

    [Fact]
    public async Task CreateDocument_WithNonExistentFolder_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await PostAsync(Urls.Documents, new { Title = "Orphan Document", FolderId = Guid.NewGuid() });

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.NotFound, expectedErrorMessage: "not found");
    }

    [Fact]
    public async Task CreateDocument_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        ClearAuthorizationHeader();

        // Act
        var response = await PostAsync(Urls.Documents, new { Title = "Test Document" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateDocument_WithEmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await PostAsync(Urls.Documents, new { Title = "" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Get Documents

    [Fact]
    public async Task GetDocuments_WithNoDocuments_ReturnsEmptyList()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await GetAsync(Urls.Documents);

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<PagedResponse<DocumentListResponse>>(HttpStatusCode.OK);
        Assert.Empty(apiResponse.Data!.Items);
        Assert.Equal(0, apiResponse.Data.TotalCount);
    }

    [Fact]
    public async Task GetDocuments_WithDocuments_ReturnsPaginatedList()
    {
        // Arrange
        await AuthenticateAsync();
        for (int i = 1; i <= 5; i++)
            await CreateDocumentAsync($"Document {i}");

        // Act
        var response = await GetAsync($"{Urls.Documents}?pageSize=3&pageNumber=1");

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<PagedResponse<DocumentListResponse>>(HttpStatusCode.OK);
        Assert.Equal(3, apiResponse.Data!.Items.Count);
        Assert.Equal(5, apiResponse.Data.TotalCount);
    }

    [Fact]
    public async Task GetDocuments_SecondPage_ReturnsRemainingItems()
    {
        // Arrange
        await AuthenticateAsync();
        for (int i = 1; i <= 5; i++)
            await CreateDocumentAsync($"Document {i}");

        // Act
        var response = await GetAsync($"{Urls.Documents}?pageSize=3&page=2");

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<PagedResponse<DocumentListResponse>>(HttpStatusCode.OK);
        Assert.Equal(2, apiResponse.Data!.Items.Count);
        Assert.Equal(5, apiResponse.Data.TotalCount);
    }

    [Fact]
    public async Task GetDocuments_FilterByStatus_ReturnsFilteredList()
    {
        // Arrange
        await AuthenticateAsync();
        await CreateDocumentAsync("Draft Document");

        // Act
        var response = await GetAsync($"{Urls.Documents}?status=Draft");

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<PagedResponse<DocumentListResponse>>(HttpStatusCode.OK);
        Assert.All(apiResponse.Data!.Items, doc => Assert.Equal("Draft", doc.Status));
    }

    [Fact]
    public async Task GetDocuments_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        ClearAuthorizationHeader();

        // Act
        var response = await GetAsync(Urls.Documents);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Get Document By Id

    [Fact]
    public async Task GetDocumentById_ExistingDocument_ReturnsDocument()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentAsync("My Document");

        // Act
        var response = await GetAsync(Urls.Document(document.Id));

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<DocumentDetailResponse>(HttpStatusCode.OK);
        Assert.Equal(document.Id, apiResponse.Data!.Id);
        Assert.Equal("My Document", apiResponse.Data.Title);
        Assert.Equal("Draft", apiResponse.Data.Status);
    }

    [Fact]
    public async Task GetDocumentById_NonExistentDocument_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await GetAsync(Urls.Document(Guid.NewGuid()));

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.NotFound, expectedErrorMessage: "not found");
    }

    [Fact]
    public async Task GetDocumentById_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        ClearAuthorizationHeader();

        // Act
        var response = await GetAsync(Urls.Document(Guid.NewGuid()));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Update Document

    [Fact]
    public async Task UpdateDocument_DraftDocument_ReturnsUpdated()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentAsync("Original Title");

        // Act
        var response = await PutAsync(Urls.Document(document.Id), new { Title = "Updated Title", Description = "New" });

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<UpdateDocumentResponse>(HttpStatusCode.OK);
        Assert.Equal("Updated Title", apiResponse.Data!.Title);
    }

    [Fact]
    public async Task UpdateDocument_NonExistentDocument_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await PutAsync(Urls.Document(Guid.NewGuid()), new { Title = "Updated Title" });

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.NotFound, expectedErrorMessage: "not found");
    }

    [Fact]
    public async Task UpdateDocument_OwnedByAnotherUser_ReturnsForbidden()
    {
        // Arrange
        await AuthenticateAsync("owner@test.com");
        var document = await CreateDocumentAsync("Owner's Document");

        await AuthenticateAsync("other@test.com");

        // Act
        var response = await PutAsync(Urls.Document(document.Id), new { Title = "Hacked Title" });

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateDocument_WithEmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentAsync("Original");

        // Act
        var response = await PutAsync(Urls.Document(document.Id), new { Title = "" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Delete Document

    [Fact]
    public async Task DeleteDocument_OwnDocument_ReturnsNoContent()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentAsync("To Delete");

        // Act
        var response = await DeleteAsync(Urls.Document(document.Id));

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify deletion
        var getResponse = await GetAsync(Urls.Document(document.Id));
        await getResponse.ValidateFailureAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteDocument_NonExistentDocument_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await DeleteAsync(Urls.Document(Guid.NewGuid()));

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.NotFound, expectedErrorMessage: "not found");
    }

    [Fact]
    public async Task DeleteDocument_OwnedByAnotherUser_ReturnsForbidden()
    {
        // Arrange
        await AuthenticateAsync("owner@test.com");
        var document = await CreateDocumentAsync("Owner's Document");

        await AuthenticateAsync("other@test.com");

        // Act
        var response = await DeleteAsync(Urls.Document(document.Id));

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Get Document History

    [Fact]
    public async Task GetDocumentHistory_ExistingDocument_ReturnsAuditLogs()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentAsync("Document with History");

        // Act
        var response = await GetAsync(Urls.DocumentHistory(document.Id));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetDocumentHistory_NonExistentDocument_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await GetAsync(Urls.DocumentHistory(Guid.NewGuid()));

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.NotFound);
    }

    #endregion

    #region Upload Version

    [Fact]
    public async Task UploadVersion_ToDraftDocument_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentAsync("Document for Version");

        // Act
        var version = await UploadVersionAsync(document.Id, "test-document.pdf");

        // Assert
        Assert.Equal(1, version.VersionNumber);
        Assert.Equal("test-document.pdf", version.FileName);
    }

    [Fact]
    public async Task UploadVersion_MultipleVersions_IncrementsVersionNumber()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentAsync("Multi-version Doc");
        await UploadVersionAsync(document.Id, "v1.pdf");

        // Act
        var version = await UploadVersionAsync(document.Id, "v2.pdf");

        // Assert
        Assert.Equal(2, version.VersionNumber);
    }

    [Fact]
    public async Task UploadVersion_ToNonExistentDocument_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();
        using var content = CreatePdfMultipartContent("test.pdf");

        // Act
        var response = await PostMultipartAsync(Urls.DocumentVersions(Guid.NewGuid()), content);

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UploadVersion_ToOtherUsersDocument_ReturnsForbidden()
    {
        // Arrange
        await AuthenticateAsync("owner@test.com");
        var document = await CreateDocumentAsync("Owner's Doc");

        await AuthenticateAsync("other@test.com");
        using var content = CreatePdfMultipartContent("malicious.pdf");

        // Act
        var response = await PostMultipartAsync(Urls.DocumentVersions(document.Id), content);

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.Forbidden);
    }

    private static MultipartFormDataContent CreatePdfMultipartContent(string fileName)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", fileName);
        return content;
    }

    #endregion
}
