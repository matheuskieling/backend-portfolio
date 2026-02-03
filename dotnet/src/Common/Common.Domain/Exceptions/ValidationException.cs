namespace Common.Domain.Exceptions;

/// <summary>
/// Base exception for validation/business rule errors (HTTP 400).
/// </summary>
public abstract class ValidationException : DomainException
{
    protected ValidationException(string code, string message)
        : base(code, message) { }
}
