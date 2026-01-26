using Identity.Domain.Exceptions;

namespace Identity.Domain.ValueObjects;

public sealed class PasswordHash : IEquatable<PasswordHash>
{
    public string Value { get; }

    private PasswordHash(string value)
    {
        Value = value;
    }

    public static PasswordHash Create(string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
            throw new InvalidPasswordException("Password hash cannot be empty.");

        return new PasswordHash(hashedPassword);
    }

    public bool Equals(PasswordHash? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is PasswordHash other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString() => "****";

    public static bool operator ==(PasswordHash? left, PasswordHash? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(PasswordHash? left, PasswordHash? right)
    {
        return !(left == right);
    }

    public static implicit operator string(PasswordHash hash) => hash.Value;
}
