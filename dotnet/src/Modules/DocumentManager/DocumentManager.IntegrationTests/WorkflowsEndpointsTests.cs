using System.Net;
using Common.IntegrationTests;
using DocumentManager.IntegrationTests.Infrastructure;
using Portfolio.Api.Contracts.DocumentManager;
using Xunit;

namespace DocumentManager.IntegrationTests;

public class WorkflowsEndpointsTests : IntegrationTestBase
{

    #region Create Workflow

    [Fact]
    public async Task CreateWorkflow_WithValidData_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var workflow = await CreateWorkflowAsync("Standard Approval", (1, "REVIEWER"), (2, "MANAGER"));

        // Assert
        Assert.NotEqual(Guid.Empty, workflow.Id);
        Assert.Equal("Standard Approval", workflow.Name);
        Assert.Equal(2, workflow.StepCount);
    }

    [Fact]
    public async Task CreateWorkflow_WithSingleStep_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var workflow = await CreateWorkflowAsync("Quick Approval", (1, "APPROVER"));

        // Assert
        Assert.Equal(1, workflow.StepCount);
    }

    [Fact]
    public async Task CreateWorkflow_WithNoSteps_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await PostAsync(Urls.Workflows, new { Name = "Empty Workflow", Steps = Array.Empty<object>() });

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<CreateWorkflowResponse>(HttpStatusCode.Created);
        Assert.Equal(0, apiResponse.Data!.StepCount);
    }

    [Fact]
    public async Task CreateWorkflow_WithManySteps_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var workflow = await CreateWorkflowAsync("Complex Workflow",
            (1, "ANALYST"), (2, "REVIEWER"), (3, "MANAGER"), (4, "DIRECTOR"), (5, "LEGAL"));

        // Assert
        Assert.Equal(5, workflow.StepCount);
    }

    [Fact]
    public async Task CreateWorkflow_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await PostAsync(Urls.Workflows, new
        {
            Name = "",
            Steps = new[] { new { StepOrder = 1, RequiredRole = "REVIEWER" } }
        });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkflow_WithDuplicateStepOrders_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await PostAsync(Urls.Workflows, new
        {
            Name = "Invalid Workflow",
            Steps = new[]
            {
                new { StepOrder = 1, RequiredRole = "REVIEWER" },
                new { StepOrder = 1, RequiredRole = "MANAGER" } // Duplicate order
            }
        });

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.BadRequest, expectedErrorMessage: "duplicate");
    }

    [Fact]
    public async Task CreateWorkflow_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        ClearAuthorizationHeader();

        // Act
        var response = await PostAsync(Urls.Workflows, new
        {
            Name = "Workflow",
            Steps = new[] { new { StepOrder = 1, RequiredRole = "REVIEWER" } }
        });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkflow_StepsOrderedCorrectly()
    {
        // Arrange
        await AuthenticateAsync();

        // Steps provided out of order
        var response = await PostAsync(Urls.Workflows, new
        {
            Name = "Unordered Steps Workflow",
            Steps = new[]
            {
                new { StepOrder = 3, RequiredRole = "DIRECTOR", Name = "Step 3" },
                new { StepOrder = 1, RequiredRole = "ANALYST", Name = "Step 1" },
                new { StepOrder = 2, RequiredRole = "MANAGER", Name = "Step 2" }
            }
        });

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<CreateWorkflowResponse>(HttpStatusCode.Created);
        Assert.Equal(3, apiResponse.Data!.StepCount);
    }

    #endregion

    #region Get Workflows

    [Fact]
    public async Task GetWorkflows_WithNoWorkflows_ReturnsEmptyList()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await GetAsync(Urls.Workflows);

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<IReadOnlyList<WorkflowListResponse>>(HttpStatusCode.OK);
        Assert.Empty(apiResponse.Data!);
    }

    [Fact]
    public async Task GetWorkflows_WithWorkflows_ReturnsList()
    {
        // Arrange
        await AuthenticateAsync();
        await CreateWorkflowAsync("Workflow 1", (1, "REVIEWER"));
        await CreateWorkflowAsync("Workflow 2", (1, "MANAGER"));

        // Act
        var response = await GetAsync(Urls.Workflows);

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<IReadOnlyList<WorkflowListResponse>>(HttpStatusCode.OK);
        Assert.Equal(2, apiResponse.Data!.Count);
    }

    [Fact]
    public async Task GetWorkflows_ReturnsOnlyActiveWorkflows()
    {
        // Arrange
        await AuthenticateAsync();
        await CreateWorkflowAsync("Active Workflow", (1, "REVIEWER"));

        // Act
        var response = await GetAsync(Urls.Workflows);

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<IReadOnlyList<WorkflowListResponse>>(HttpStatusCode.OK);
        Assert.All(apiResponse.Data!, w => Assert.True(w.IsActive));
    }

    [Fact]
    public async Task GetWorkflows_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        ClearAuthorizationHeader();

        // Act
        var response = await GetAsync(Urls.Workflows);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}
