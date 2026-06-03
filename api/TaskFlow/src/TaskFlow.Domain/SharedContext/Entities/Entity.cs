namespace TaskFlow.Domain.SharedContext.Entities;

public abstract class Entity: IEquatable<Guid>
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; protected set; }
    public DateTime? DeletedAtUtc { get; protected set; }
    
    public bool IsDeleted => DeletedAtUtc.HasValue;
    
    protected void SoftDelete()
    {
        DeletedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }
    
    public bool Equals(Guid id) => Id == id;

    public override int GetHashCode() => Id.GetHashCode();
}