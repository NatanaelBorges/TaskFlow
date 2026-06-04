using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.TaskContext.Interfaces;
using TaskFlow.Domain.TaskContext.Entities;
using TaskFlow.Domain.TaskContext.Enums;
using TaskFlow.Infrastructure.SharedContext.Data;
using TaskFlow.Infrastructure.SharedContext.Repositories;

namespace TaskFlow.Infrastructure.TaskContext.Repositories;

public sealed class TaskRepository(AppDbContext context)
    : Repository<TaskItem, Guid>(context), ITaskRepository
{
    public async Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetAllAsync(
        TaskItemStatus? statusFilter = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable().Where(t => t.DeletedAtUtc == null);

        if (statusFilter.HasValue)
            query = query.Where(t => t.Status == statusFilter.Value);

        query = query.OrderByDescending(t => t.CreatedAtUtc);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items.AsReadOnly(), totalCount);
    }
}