namespace DocumentManager.IntegrationTests.Infrastructure;

public static class Urls
{
    private const string Base = "/api/document-manager";

    // Folders
    public const string Folders = $"{Base}/folders";
    public const string FolderTree = $"{Base}/folders/tree";

    // Documents
    public const string Documents = $"{Base}/documents";
    public static string Document(Guid id) => $"{Documents}/{id}";
    public static string DocumentHistory(Guid id) => $"{Documents}/{id}/history";
    public static string DocumentVersions(Guid id) => $"{Documents}/{id}/versions";
    public static string DocumentTags(Guid id) => $"{Documents}/{id}/tags";
    public static string DocumentTag(Guid docId, Guid tagId) => $"{Documents}/{docId}/tags/{tagId}";
    public static string SubmitDocument(Guid id) => $"{Documents}/{id}/submit";

    // Tags
    public const string Tags = $"{Base}/tags";

    // Workflows
    public const string Workflows = $"{Base}/workflows";

    // Approvals
    public const string Approvals = $"{Base}/approvals";
    public static string Approval(Guid id) => $"{Approvals}/{id}";
    public static string ApproveStep(Guid id) => $"{Approvals}/{id}/approve";
    public static string RejectStep(Guid id) => $"{Approvals}/{id}/reject";

    // Identity
    public const string Register = "/api/identity/register";
    public const string Login = "/api/identity/login";
}
