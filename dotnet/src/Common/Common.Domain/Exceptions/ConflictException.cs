namespace Common.Domain.Exceptions;

/// <summary>
/// Base exception for conflict errors (HTTP 409).
/// </summary>
public abstract class ConflictException : DomainException
{
    protected ConflictException(string code, string message)
        : base(code, message) { }
}
