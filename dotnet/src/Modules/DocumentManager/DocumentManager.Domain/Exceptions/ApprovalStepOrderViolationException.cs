using Common.Domain;

namespace DocumentManager.Domain.Exceptions;

public sealed class ApprovalStepOrderViolationException : DomainException
{
    private const string ErrorCode = "APPROVAL_STEP_ORDER_VIOLATION";

    public ApprovalStepOrderViolationException(int currentStep, int attemptedStep)
        : base(ErrorCode, $"Cannot process step {attemptedStep}. Current step is {currentStep}. Steps must be processed in order.")
    {
    }
}
