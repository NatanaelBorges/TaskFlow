using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.SharedContext.Interfaces;
using TaskFlow.Domain.SharedContext.Entities;

namespace TaskFlow.Infrastructure.SharedContext.Repositories;

public abstract class Repository<T, TId>(DbContext context) : IRepository<T, TId>
    where T : Entity
    where TId : notnull
{
    protected readonly DbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public async Task<T?> GetByIdAsync(TId id, CancellationToken ct = default)
    {
        var entity = await DbSet.FindAsync([id], ct);
        return entity is { DeletedAtUtc: null } ? entity : null;
    }
    
    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await DbSet.AddAsync(entity, ct);
        await Context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<T?> UpdateAsync(T entity, CancellationToken ct = default)
    {
        DbSet.Update(entity);
        await Context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<bool> DeleteAsync(TId id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);
        if (entity is null) return false;

        DbSet.Remove(entity);
        await Context.SaveChangesAsync(ct);
        return true;
    }
}
