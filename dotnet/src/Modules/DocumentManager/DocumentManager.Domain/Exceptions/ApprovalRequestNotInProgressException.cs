using Common.Domain.Exceptions;
using DocumentManager.Domain.Enums;

namespace DocumentManager.Domain.Exceptions;

public sealed class ApprovalRequestNotInProgressException : ValidationException
{
    private const string ErrorCode = "APPROVAL_REQUEST_NOT_IN_PROGRESS";

    public ApprovalRequestNotInProgressException(Guid approvalRequestId, ApprovalRequestStatus currentStatus)
        : base(ErrorCode, $"Approval request '{approvalRequestId}' is in '{currentStatus}' status and cannot be modified.")
    {
    }
}
