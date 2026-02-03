using Common.Domain.Exceptions;

namespace DocumentManager.Domain.Exceptions;

public sealed class ApprovalStepOrderViolationException : ValidationException
{
    private const string ErrorCode = "APPROVAL_STEP_ORDER_VIOLATION";

    public ApprovalStepOrderViolationException(int currentStep, int attemptedStep)
        : base(ErrorCode, $"Cannot process step {attemptedStep}. Current step is {currentStep}. Steps must be processed in order.")
    {
    }
}
