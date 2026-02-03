namespace Common.Domain.Exceptions;

/// <summary>
/// Base exception for forbidden access errors (HTTP 403).
/// </summary>
public abstract class ForbiddenException : DomainException
{
    protected ForbiddenException(string code, string message)
        : base(code, message) { }
}
