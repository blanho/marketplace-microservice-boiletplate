namespace SharedKernel;

/// <summary>
/// Base class for all domain entities. Provides typed identity and structural equality by Id.
/// </summary>
public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; set; } = default!;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
        left is not null && right is not null && left.Equals(right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !(left == right);
}

