using DocumentManager.Domain.Exceptions;

namespace DocumentManager.Domain.ValueObjects;

public sealed class MimeType : IEquatable<MimeType>
{
    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "text/plain",
        "image/png",
        "image/jpeg"
    };

    public string Value { get; }

    private MimeType(string value)
    {
        Value = value.ToLowerInvariant();
    }

    public static MimeType Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidMimeTypeException("MIME type cannot be empty.");

        var trimmed = value.Trim();

        if (!AllowedMimeTypes.Contains(trimmed))
            throw new InvalidMimeTypeException($"MIME type '{trimmed}' is not allowed. Allowed types: {string.Join(", ", AllowedMimeTypes)}");

        return new MimeType(trimmed);
    }

    public static bool IsAllowed(string mimeType)
    {
        return !string.IsNullOrWhiteSpace(mimeType) && AllowedMimeTypes.Contains(mimeType.Trim());
    }

    public bool Equals(MimeType? other)
    {
        if (other is null) return false;
        return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is MimeType other && Equals(other);
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    }

    public override string ToString() => Value;

    public static bool operator ==(MimeType? left, MimeType? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(MimeType? left, MimeType? right)
    {
        return !(left == right);
    }

    public static implicit operator string(MimeType mimeType) => mimeType.Value;
}
