using Common.Domain.Exceptions;

namespace DocumentManager.Domain.Exceptions;

public sealed class ApprovalRequestNotFoundException : NotFoundException
{
    private const string ErrorCode = "APPROVAL_REQUEST_NOT_FOUND";

    public ApprovalRequestNotFoundException(Guid approvalRequestId)
        : base(ErrorCode, $"Approval request with ID '{approvalRequestId}' was not found.")
    {
    }
}
