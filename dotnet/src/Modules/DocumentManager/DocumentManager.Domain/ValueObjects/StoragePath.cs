using DocumentManager.Domain.Exceptions;

namespace DocumentManager.Domain.ValueObjects;

public sealed class StoragePath : IEquatable<StoragePath>
{
    private const int MaxLength = 1024;

    public string Value { get; }

    private StoragePath(string value)
    {
        Value = value;
    }

    public static StoragePath Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidStoragePathException("Storage path cannot be empty.");

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
            throw new InvalidStoragePathException($"Storage path cannot exceed {MaxLength} characters.");

        // Basic path traversal prevention
        if (trimmed.Contains(".."))
            throw new InvalidStoragePathException("Storage path cannot contain path traversal sequences.");

        return new StoragePath(trimmed);
    }

    public bool Equals(StoragePath? other)
    {
        if (other is null) return false;
        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return obj is StoragePath other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString() => Value;

    public static bool operator ==(StoragePath? left, StoragePath? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(StoragePath? left, StoragePath? right)
    {
        return !(left == right);
    }

    public static implicit operator string(StoragePath storagePath) => storagePath.Value;
}
