using DocumentManager.Application.Common.Interfaces;
using DocumentManager.Domain.Entities;
using DocumentManager.Domain.Enums;

namespace DocumentManager.Infrastructure.Services;

public class DocumentAuthorizationService : IDocumentAuthorizationService
{
    private const string AdminRole = "DOCUMENT_ADMIN";
    private const string ManageAllPermission = "document:manage_all";
    private const string ReadAllPermission = "document:read_all";

    public bool CanRead(Document document, Guid userId, IEnumerable<string> userRoles)
    {
        // Owner can always read
        if (document.IsOwnedBy(userId))
            return true;

        // Admin or users with read_all permission can read
        var rolesList = userRoles.ToList();
        if (rolesList.Contains(AdminRole, StringComparer.OrdinalIgnoreCase))
            return true;

        // Approved documents are public
        if (document.Status == DocumentStatus.Approved)
            return true;

        return false;
    }

    public bool CanModify(Document document, Guid userId, IEnumerable<string> userRoles)
    {
        // Only draft documents can be modified
        if (document.Status != DocumentStatus.Draft)
            return false;

        // Owner can modify their drafts
        if (document.IsOwnedBy(userId))
            return true;

        // Admin can modify any draft
        var rolesList = userRoles.ToList();
        if (rolesList.Contains(AdminRole, StringComparer.OrdinalIgnoreCase))
            return true;

        return false;
    }

    public bool CanDelete(Document document, Guid userId, IEnumerable<string> userRoles)
    {
        // Owner can delete their own documents
        if (document.IsOwnedBy(userId))
            return true;

        // Admin can delete any document
        var rolesList = userRoles.ToList();
        if (rolesList.Contains(AdminRole, StringComparer.OrdinalIgnoreCase))
            return true;

        return false;
    }

    public bool CanApprove(ApprovalRequest approvalRequest, Guid userId, IEnumerable<string> userRoles)
    {
        // Request must be in progress
        if (approvalRequest.Status != ApprovalRequestStatus.InProgress)
            return false;

        var currentStep = approvalRequest.GetCurrentStep();
        if (currentStep == null)
            return false;

        // User must have the required role for the current step
        var rolesList = userRoles.ToList();
        if (rolesList.Contains(currentStep.RequiredRole, StringComparer.OrdinalIgnoreCase))
            return true;

        // Admin can approve any step
        if (rolesList.Contains(AdminRole, StringComparer.OrdinalIgnoreCase))
            return true;

        return false;
    }
}
