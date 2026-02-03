namespace Common.Domain.Exceptions;

/// <summary>
/// Base exception for resource not found errors (HTTP 404).
/// </summary>
public abstract class NotFoundException : DomainException
{
    protected NotFoundException(string code, string message)
        : base(code, message) { }
}
