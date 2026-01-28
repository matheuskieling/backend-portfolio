using System.Net;
using Common.IntegrationTests;
using DocumentManager.IntegrationTests.Infrastructure;
using Portfolio.Api.Contracts.DocumentManager;
using Xunit;

namespace DocumentManager.IntegrationTests;

public class ApprovalsEndpointsTests : IntegrationTestBase
{
    public ApprovalsEndpointsTests(DocumentManagerWebApplicationFactory factory) : base(factory)
    {
    }

    #region Submit for Approval

    [Fact]
    public async Task SubmitForApproval_ValidDocumentWithVersion_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentWithVersionAsync();
        var workflow = await CreateWorkflowAsync();

        // Act
        var approval = await SubmitForApprovalAsync(document.Id, workflow.Id);

        // Assert
        Assert.NotEqual(Guid.Empty, approval.ApprovalRequestId);
        Assert.Equal(document.Id, approval.DocumentId);
    }

    [Fact]
    public async Task SubmitForApproval_DocumentWithoutVersion_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentAsync("No Version Doc");
        var workflow = await CreateWorkflowAsync();

        // Act
        var response = await PostAsync(Urls.SubmitDocument(document.Id), new { WorkflowId = workflow.Id });

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.BadRequest, expectedErrorMessage: "version");
    }

    [Fact]
    public async Task SubmitForApproval_NonExistentDocument_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();
        var workflow = await CreateWorkflowAsync();

        // Act
        var response = await PostAsync(Urls.SubmitDocument(Guid.NewGuid()), new { WorkflowId = workflow.Id });

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.NotFound, expectedErrorMessage: "document");
    }

    [Fact]
    public async Task SubmitForApproval_NonExistentWorkflow_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentWithVersionAsync();

        // Act
        var response = await PostAsync(Urls.SubmitDocument(document.Id), new { WorkflowId = Guid.NewGuid() });

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.NotFound, expectedErrorMessage: "workflow");
    }

    [Fact]
    public async Task SubmitForApproval_AlreadySubmittedDocument_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentWithVersionAsync();
        var workflow = await CreateWorkflowAsync();
        await SubmitForApprovalAsync(document.Id, workflow.Id);

        // Act - Submit again
        var response = await PostAsync(Urls.SubmitDocument(document.Id), new { WorkflowId = workflow.Id });

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.BadRequest, expectedErrorMessage: "draft");
    }

    [Fact]
    public async Task SubmitForApproval_OtherUsersDocument_ReturnsForbidden()
    {
        // Arrange
        await AuthenticateAsync("owner@test.com");
        var document = await CreateDocumentWithVersionAsync();
        var workflow = await CreateWorkflowAsync();

        await AuthenticateAsync("other@test.com");

        // Act
        var response = await PostAsync(Urls.SubmitDocument(document.Id), new { WorkflowId = workflow.Id });

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SubmitForApproval_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        ClearAuthorizationHeader();

        // Act
        var response = await PostAsync(Urls.SubmitDocument(Guid.NewGuid()), new { WorkflowId = Guid.NewGuid() });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Approve Step

    [Fact]
    public async Task ApproveStep_ValidRequest_ReturnsOk()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentWithVersionAsync();
        var workflow = await CreateWorkflowAsync("Single Step", (1, "REVIEWER"));
        var approval = await SubmitForApprovalAsync(document.Id, workflow.Id);

        // Act
        var response = await PostAsync(Urls.ApproveStep(approval.ApprovalRequestId), new { Comment = "Looks good!" });

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<ApproveStepResponse>(HttpStatusCode.OK);
        Assert.Equal(1, apiResponse.Data!.StepApproved);
    }

    [Fact]
    public async Task ApproveStep_WithoutComment_ReturnsOk()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentWithVersionAsync();
        var workflow = await CreateWorkflowAsync();
        var approval = await SubmitForApprovalAsync(document.Id, workflow.Id);

        // Act
        var response = await PostAsync(Urls.ApproveStep(approval.ApprovalRequestId), new { });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ApproveStep_MultiStepWorkflow_ProgressesToNextStep()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentWithVersionAsync();
        var workflow = await CreateWorkflowAsync("Two Step", (1, "REVIEWER"), (2, "MANAGER"));
        var approval = await SubmitForApprovalAsync(document.Id, workflow.Id);

        // Approve first step
        var firstResponse = await PostAsync(Urls.ApproveStep(approval.ApprovalRequestId), new { });
        var firstResult = (await firstResponse.ValidateSuccessAsync<ApproveStepResponse>(HttpStatusCode.OK)).Data!;
        Assert.Equal(1, firstResult.StepApproved);
        Assert.Equal("InProgress", firstResult.NewStatus);

        // Act - Approve second step
        var secondResponse = await PostAsync(Urls.ApproveStep(approval.ApprovalRequestId), new { });

        // Assert
        var secondResult = (await secondResponse.ValidateSuccessAsync<ApproveStepResponse>(HttpStatusCode.OK)).Data!;
        Assert.Equal(2, secondResult.StepApproved);
        Assert.Equal("Approved", secondResult.NewStatus);
    }

    [Fact]
    public async Task ApproveStep_NonExistentApprovalRequest_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await PostAsync(Urls.ApproveStep(Guid.NewGuid()), new { });

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.NotFound, expectedErrorMessage: "approval");
    }

    [Fact]
    public async Task ApproveStep_AlreadyCompletedWorkflow_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentWithVersionAsync();
        var workflow = await CreateWorkflowAsync("Single Step", (1, "REVIEWER"));
        var approval = await SubmitForApprovalAsync(document.Id, workflow.Id);

        // Complete the workflow
        await PostAsync(Urls.ApproveStep(approval.ApprovalRequestId), new { });

        // Act - Try to approve again
        var response = await PostAsync(Urls.ApproveStep(approval.ApprovalRequestId), new { });

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.BadRequest, expectedErrorMessage: "in progress");
    }

    [Fact]
    public async Task ApproveStep_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        ClearAuthorizationHeader();

        // Act
        var response = await PostAsync(Urls.ApproveStep(Guid.NewGuid()), new { });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Reject Step

    [Fact]
    public async Task RejectStep_ValidRequest_ReturnsOk()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentWithVersionAsync();
        var workflow = await CreateWorkflowAsync();
        var approval = await SubmitForApprovalAsync(document.Id, workflow.Id);

        // Act
        var response = await PostAsync(Urls.RejectStep(approval.ApprovalRequestId), new { Comment = "Needs work" });

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<RejectStepResponse>(HttpStatusCode.OK);
        Assert.Equal(1, apiResponse.Data!.StepRejected);
        Assert.Equal("Rejected", apiResponse.Data.NewStatus);
    }

    [Fact]
    public async Task RejectStep_MultiStepWorkflow_ImmediatelyEndsWorkflow()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentWithVersionAsync();
        var workflow = await CreateWorkflowAsync("Three Step", (1, "REVIEWER"), (2, "MANAGER"), (3, "DIRECTOR"));
        var approval = await SubmitForApprovalAsync(document.Id, workflow.Id);

        // Approve first step
        await PostAsync(Urls.ApproveStep(approval.ApprovalRequestId), new { });

        // Act - Reject at second step
        var response = await PostAsync(Urls.RejectStep(approval.ApprovalRequestId), new { Comment = "Not acceptable" });

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<RejectStepResponse>(HttpStatusCode.OK);
        Assert.Equal("Rejected", apiResponse.Data!.NewStatus);
    }

    [Fact]
    public async Task RejectStep_NonExistentApprovalRequest_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await PostAsync(Urls.RejectStep(Guid.NewGuid()), new { });

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.NotFound, expectedErrorMessage: "approval");
    }

    [Fact]
    public async Task RejectStep_AlreadyRejectedWorkflow_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentWithVersionAsync();
        var workflow = await CreateWorkflowAsync();
        var approval = await SubmitForApprovalAsync(document.Id, workflow.Id);

        // Reject once
        await PostAsync(Urls.RejectStep(approval.ApprovalRequestId), new { });

        // Act - Try to reject again
        var response = await PostAsync(Urls.RejectStep(approval.ApprovalRequestId), new { });

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.BadRequest, expectedErrorMessage: "in progress");
    }

    [Fact]
    public async Task RejectStep_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        ClearAuthorizationHeader();

        // Act
        var response = await PostAsync(Urls.RejectStep(Guid.NewGuid()), new { });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Get Approval Status

    [Fact]
    public async Task GetApprovalStatus_ExistingApproval_ReturnsStatus()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentWithVersionAsync();
        var workflow = await CreateWorkflowAsync("Two Step", (1, "REVIEWER"), (2, "MANAGER"));
        var approval = await SubmitForApprovalAsync(document.Id, workflow.Id);

        // Act
        var response = await GetAsync(Urls.Approval(approval.ApprovalRequestId));

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<ApprovalStatusResponse>(HttpStatusCode.OK);
        Assert.Equal(approval.ApprovalRequestId, apiResponse.Data!.Id);
        Assert.Equal("InProgress", apiResponse.Data.Status);
        Assert.Equal(1, apiResponse.Data.CurrentStepOrder);
        Assert.Equal(2, apiResponse.Data.TotalSteps);
    }

    [Fact]
    public async Task GetApprovalStatus_AfterApproval_ShowsProgress()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentWithVersionAsync();
        var workflow = await CreateWorkflowAsync("Two Step", (1, "REVIEWER"), (2, "MANAGER"));
        var approval = await SubmitForApprovalAsync(document.Id, workflow.Id);

        // Approve first step
        await PostAsync(Urls.ApproveStep(approval.ApprovalRequestId), new { });

        // Act
        var response = await GetAsync(Urls.Approval(approval.ApprovalRequestId));

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<ApprovalStatusResponse>(HttpStatusCode.OK);
        Assert.Equal(2, apiResponse.Data!.CurrentStepOrder);
        Assert.Equal("InProgress", apiResponse.Data.Status);
    }

    [Fact]
    public async Task GetApprovalStatus_FullyApproved_ShowsApprovedStatus()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentWithVersionAsync();
        var workflow = await CreateWorkflowAsync("Single Step", (1, "REVIEWER"));
        var approval = await SubmitForApprovalAsync(document.Id, workflow.Id);

        await PostAsync(Urls.ApproveStep(approval.ApprovalRequestId), new { });

        // Act
        var response = await GetAsync(Urls.Approval(approval.ApprovalRequestId));

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<ApprovalStatusResponse>(HttpStatusCode.OK);
        Assert.Equal("Approved", apiResponse.Data!.Status);
    }

    [Fact]
    public async Task GetApprovalStatus_Rejected_ShowsRejectedStatus()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentWithVersionAsync();
        var workflow = await CreateWorkflowAsync();
        var approval = await SubmitForApprovalAsync(document.Id, workflow.Id);

        await PostAsync(Urls.RejectStep(approval.ApprovalRequestId), new { Comment = "Rejected" });

        // Act
        var response = await GetAsync(Urls.Approval(approval.ApprovalRequestId));

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<ApprovalStatusResponse>(HttpStatusCode.OK);
        Assert.Equal("Rejected", apiResponse.Data!.Status);
    }

    [Fact]
    public async Task GetApprovalStatus_NonExistentApproval_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await GetAsync(Urls.Approval(Guid.NewGuid()));

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.NotFound, expectedErrorMessage: "approval");
    }

    [Fact]
    public async Task GetApprovalStatus_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        ClearAuthorizationHeader();

        // Act
        var response = await GetAsync(Urls.Approval(Guid.NewGuid()));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public async Task FullApprovalWorkflow_CreateSubmitApprove_DocumentBecomesApproved()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentWithVersionAsync("Important Contract");
        var workflow = await CreateWorkflowAsync("Contract Approval", (1, "LEGAL"), (2, "EXECUTIVE"));
        var approval = await SubmitForApprovalAsync(document.Id, workflow.Id);

        // Verify document status changed
        var docResponse1 = await GetAsync(Urls.Document(document.Id));
        var doc1 = (await docResponse1.ValidateSuccessAsync<DocumentDetailResponse>(HttpStatusCode.OK)).Data!;
        Assert.Equal("PendingApproval", doc1.Status);

        // Approve both steps
        await PostAsync(Urls.ApproveStep(approval.ApprovalRequestId), new { Comment = "Legal approved" });
        await PostAsync(Urls.ApproveStep(approval.ApprovalRequestId), new { Comment = "Executive approved" });

        // Act - Verify final document status
        var docResponse2 = await GetAsync(Urls.Document(document.Id));
        var doc2 = (await docResponse2.ValidateSuccessAsync<DocumentDetailResponse>(HttpStatusCode.OK)).Data!;

        // Assert
        Assert.Equal("Approved", doc2.Status);
    }

    [Fact]
    public async Task RejectionWorkflow_DocumentBecomesRejected()
    {
        // Arrange
        await AuthenticateAsync();
        var document = await CreateDocumentWithVersionAsync("Rejected Document");
        var workflow = await CreateWorkflowAsync("Review", (1, "REVIEWER"));
        var approval = await SubmitForApprovalAsync(document.Id, workflow.Id);

        // Reject
        await PostAsync(Urls.RejectStep(approval.ApprovalRequestId), new { Comment = "Incomplete" });

        // Act - Verify document status
        var docResponse = await GetAsync(Urls.Document(document.Id));
        var doc = (await docResponse.ValidateSuccessAsync<DocumentDetailResponse>(HttpStatusCode.OK)).Data!;

        // Assert
        Assert.Equal("Rejected", doc.Status);
    }

    #endregion
}
