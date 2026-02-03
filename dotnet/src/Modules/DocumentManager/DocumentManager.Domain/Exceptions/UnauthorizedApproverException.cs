using Common.Domain.Exceptions;

namespace DocumentManager.Domain.Exceptions;

public sealed class UnauthorizedApproverException : ForbiddenException
{
    private const string ErrorCode = "UNAUTHORIZED_APPROVER";

    public UnauthorizedApproverException(Guid userId, string requiredRole)
        : base(ErrorCode, $"User '{userId}' does not have the required role '{requiredRole}' to approve this step.")
    {
    }

    public UnauthorizedApproverException(Guid userId, Guid approvalRequestId)
        : base(ErrorCode, $"User '{userId}' is not authorized to make decisions on approval request '{approvalRequestId}'.")
    {
    }
}
