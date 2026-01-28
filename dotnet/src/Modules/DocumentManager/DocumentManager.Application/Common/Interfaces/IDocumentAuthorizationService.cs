using DocumentManager.Domain.Entities;

namespace DocumentManager.Application.Common.Interfaces;

public interface IDocumentAuthorizationService
{
    bool CanRead(Document document, Guid userId, IEnumerable<string> userRoles);
    bool CanModify(Document document, Guid userId, IEnumerable<string> userRoles);
    bool CanDelete(Document document, Guid userId, IEnumerable<string> userRoles);
    bool CanApprove(ApprovalRequest approvalRequest, Guid userId, IEnumerable<string> userRoles);
}
