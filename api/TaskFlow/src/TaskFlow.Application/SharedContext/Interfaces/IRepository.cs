namespace TaskFlow.Application.SharedContext.Interfaces;

public interface IRepository<T, in TId>
{
    Task<T?> GetByIdAsync(TId id, CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task<T?> UpdateAsync(T entity, CancellationToken ct = default);
    Task<bool> DeleteAsync(TId id, CancellationToken ct = default);
}