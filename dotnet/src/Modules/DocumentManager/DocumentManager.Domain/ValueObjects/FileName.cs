using DocumentManager.Domain.Exceptions;

namespace DocumentManager.Domain.ValueObjects;

public sealed class FileName : IEquatable<FileName>
{
    private static readonly char[] InvalidChars = Path.GetInvalidFileNameChars();
    private const int MaxLength = 255;

    public string Value { get; }

    private FileName(string value)
    {
        Value = value;
    }

    public static FileName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidFileNameException("File name cannot be empty.");

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
            throw new InvalidFileNameException($"File name cannot exceed {MaxLength} characters.");

        if (trimmed.IndexOfAny(InvalidChars) >= 0)
            throw new InvalidFileNameException($"File name contains invalid characters.");

        return new FileName(trimmed);
    }

    public bool Equals(FileName? other)
    {
        if (other is null) return false;
        return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is FileName other && Equals(other);
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    }

    public override string ToString() => Value;

    public static bool operator ==(FileName? left, FileName? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(FileName? left, FileName? right)
    {
        return !(left == right);
    }

    public static implicit operator string(FileName fileName) => fileName.Value;
}
