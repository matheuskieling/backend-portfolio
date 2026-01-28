using System.Net;
using Common.IntegrationTests;
using DocumentManager.IntegrationTests.Infrastructure;
using Portfolio.Api.Contracts.DocumentManager;
using Xunit;

namespace DocumentManager.IntegrationTests;

public class FoldersEndpointsTests : IntegrationTestBase
{
    public FoldersEndpointsTests(DocumentManagerWebApplicationFactory factory) : base(factory)
    {
    }

    #region Create Folder

    [Fact]
    public async Task CreateFolder_WithValidData_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await PostAsync(Urls.Folders, new { Name = "Test Folder" });

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<CreateFolderResponse>(HttpStatusCode.Created);
        Assert.NotEqual(Guid.Empty, apiResponse.Data!.Id);
        Assert.Equal("Test Folder", apiResponse.Data.Name);
        Assert.Null(apiResponse.Data.ParentFolderId);
    }

    [Fact]
    public async Task CreateFolder_WithParentFolder_ReturnsCreatedWithParentId()
    {
        // Arrange
        await AuthenticateAsync();
        var parent = await CreateFolderAsync("Parent Folder");

        // Act
        var response = await PostAsync(Urls.Folders, new { Name = "Child Folder", ParentFolderId = parent.Id });

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<CreateFolderResponse>(HttpStatusCode.Created);
        Assert.Equal("Child Folder", apiResponse.Data!.Name);
        Assert.Equal(parent.Id, apiResponse.Data.ParentFolderId);
    }

    [Fact]
    public async Task CreateFolder_WithNonExistentParent_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await PostAsync(Urls.Folders, new { Name = "Orphan", ParentFolderId = Guid.NewGuid() });

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.NotFound, expectedErrorMessage: "not found");
    }

    [Fact]
    public async Task CreateFolder_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        ClearAuthorizationHeader();

        // Act
        var response = await PostAsync(Urls.Folders, new { Name = "Test Folder" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateFolder_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await PostAsync(Urls.Folders, new { Name = "" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateFolder_WithWhitespaceName_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await PostAsync(Urls.Folders, new { Name = "   " });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Get Folder Tree

    [Fact]
    public async Task GetFolderTree_WithNoFolders_ReturnsEmptyList()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await GetAsync(Urls.FolderTree);

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<IReadOnlyList<FolderTreeResponse>>(HttpStatusCode.OK);
        Assert.Empty(apiResponse.Data!);
    }

    [Fact]
    public async Task GetFolderTree_WithFolders_ReturnsHierarchy()
    {
        // Arrange
        await AuthenticateAsync();
        var parent = await CreateFolderAsync("Parent");
        await CreateFolderAsync("Child 1", parent.Id);
        await CreateFolderAsync("Child 2", parent.Id);
        await CreateFolderAsync("Root Folder");

        // Act
        var response = await GetAsync(Urls.FolderTree);

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<IReadOnlyList<FolderTreeResponse>>(HttpStatusCode.OK);
        Assert.Equal(2, apiResponse.Data!.Count); // 2 root level folders
    }

    [Fact]
    public async Task GetFolderTree_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        ClearAuthorizationHeader();

        // Act
        var response = await GetAsync(Urls.FolderTree);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}
