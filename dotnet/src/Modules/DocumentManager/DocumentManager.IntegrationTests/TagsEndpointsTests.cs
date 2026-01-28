using System.Net;
using Common.IntegrationTests;
using DocumentManager.IntegrationTests.Infrastructure;
using Portfolio.Api.Contracts.DocumentManager;
using Xunit;

namespace DocumentManager.IntegrationTests;

public class TagsEndpointsTests : IntegrationTestBase
{
    public TagsEndpointsTests(DocumentManagerWebApplicationFactory factory) : base(factory)
    {
    }

    #region Create Tag

    [Fact]
    public async Task CreateTag_WithValidName_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await PostAsync(Urls.Tags, new { Name = "Important" });

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<CreateTagResponse>(HttpStatusCode.Created);
        Assert.NotEqual(Guid.Empty, apiResponse.Data!.Id);
        Assert.Equal("important", apiResponse.Data.Name); // normalized to lowercase
    }

    [Fact]
    public async Task CreateTag_NormalizesToLowercase()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await PostAsync(Urls.Tags, new { Name = "UPPERCASE TAG" });

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<CreateTagResponse>(HttpStatusCode.Created);
        Assert.Equal("uppercase tag", apiResponse.Data!.Name);
    }

    [Fact]
    public async Task CreateTag_DuplicateName_ReturnsConflict()
    {
        // Arrange
        await AuthenticateAsync();
        await CreateTagAsync("duplicate");

        // Act
        var response = await PostAsync(Urls.Tags, new { Name = "Duplicate" }); // same name, different case

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.Conflict, expectedErrorMessage: "already exists");
    }

    [Fact]
    public async Task CreateTag_EmptyName_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await PostAsync(Urls.Tags, new { Name = "" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTag_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        ClearAuthorizationHeader();

        // Act
        var response = await PostAsync(Urls.Tags, new { Name = "Test Tag" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Get Tags

    [Fact]
    public async Task GetTags_WithNoTags_ReturnsEmptyList()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await GetAsync(Urls.Tags);

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<IReadOnlyList<CreateTagResponse>>(HttpStatusCode.OK);
        Assert.Empty(apiResponse.Data!);
    }

    [Fact]
    public async Task GetTags_WithTags_ReturnsAllTags()
    {
        // Arrange
        await AuthenticateAsync();
        await CreateTagAsync("tag1");
        await CreateTagAsync("tag2");
        await CreateTagAsync("tag3");

        // Act
        var response = await GetAsync(Urls.Tags);

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<IReadOnlyList<CreateTagResponse>>(HttpStatusCode.OK);
        Assert.Equal(3, apiResponse.Data!.Count);
    }

    [Fact]
    public async Task GetTags_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        ClearAuthorizationHeader();

        // Act
        var response = await GetAsync(Urls.Tags);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Add Tag to Document

    [Fact]
    public async Task AddTagToDocument_ValidTagAndDocument_ReturnsNoContent()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentAsync("Tagged Document");
        var tag = await CreateTagAsync("mytag");

        // Act
        var response = await PostAsync(Urls.DocumentTags(document.Id), new { TagId = tag.Id });

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task AddTagToDocument_NonExistentDocument_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();
        var tag = await CreateTagAsync("orphan-tag");

        // Act
        var response = await PostAsync(Urls.DocumentTags(Guid.NewGuid()), new { TagId = tag.Id });

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.NotFound, expectedErrorMessage: "document");
    }

    [Fact]
    public async Task AddTagToDocument_NonExistentTag_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentAsync("Document");

        // Act
        var response = await PostAsync(Urls.DocumentTags(document.Id), new { TagId = Guid.NewGuid() });

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.NotFound, expectedErrorMessage: "tag");
    }

    [Fact]
    public async Task AddTagToDocument_DuplicateTag_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentAsync("Document");
        var tag = await CreateTagAsync("duplicate-tag");
        await AddTagToDocumentAsync(document.Id, tag.Id);

        // Act - Add same tag again
        var response = await PostAsync(Urls.DocumentTags(document.Id), new { TagId = tag.Id });

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.BadRequest, expectedErrorMessage: "already");
    }

    [Fact]
    public async Task AddTagToDocument_OtherUsersDocument_ReturnsForbidden()
    {
        // Arrange
        await AuthenticateAsync("owner@test.com");
        var document = await CreateDocumentAsync("Owner's Doc");
        var tag = await CreateTagAsync("tag");

        await AuthenticateAsync("other@test.com");

        // Act
        var response = await PostAsync(Urls.DocumentTags(document.Id), new { TagId = tag.Id });

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Remove Tag from Document

    [Fact]
    public async Task RemoveTagFromDocument_ExistingTag_ReturnsNoContent()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentAsync("Document");
        var tag = await CreateTagAsync("removable");
        await AddTagToDocumentAsync(document.Id, tag.Id);

        // Act
        var response = await DeleteAsync(Urls.DocumentTag(document.Id, tag.Id));

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RemoveTagFromDocument_NonExistentDocument_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();
        var tag = await CreateTagAsync("tag");

        // Act
        var response = await DeleteAsync(Urls.DocumentTag(Guid.NewGuid(), tag.Id));

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.NotFound, expectedErrorMessage: "document");
    }

    [Fact]
    public async Task RemoveTagFromDocument_NonExistentTag_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentAsync("Document");

        // Act
        var response = await DeleteAsync(Urls.DocumentTag(document.Id, Guid.NewGuid()));

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.NotFound, expectedErrorMessage: "tag");
    }

    [Fact]
    public async Task RemoveTagFromDocument_TagNotOnDocument_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentAsync("Document");
        var tag = await CreateTagAsync("not-attached");

        // Act - Remove tag that was never added
        var response = await DeleteAsync(Urls.DocumentTag(document.Id, tag.Id));

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.BadRequest, expectedErrorMessage: "not");
    }

    [Fact]
    public async Task RemoveTagFromDocument_OtherUsersDocument_ReturnsForbidden()
    {
        // Arrange
        await AuthenticateAsync("owner@test.com");
        var document = await CreateDocumentAsync("Owner's Doc");
        var tag = await CreateTagAsync("owner-tag");
        await AddTagToDocumentAsync(document.Id, tag.Id);

        await AuthenticateAsync("other@test.com");

        // Act
        var response = await DeleteAsync(Urls.DocumentTag(document.Id, tag.Id));

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.Forbidden);
    }

    #endregion
}
