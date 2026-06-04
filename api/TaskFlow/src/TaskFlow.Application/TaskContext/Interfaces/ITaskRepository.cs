using TaskFlow.Application.SharedContext.Interfaces;
using TaskFlow.Domain.TaskContext.Entities;
using TaskFlow.Domain.TaskContext.Enums;

namespace TaskFlow.Application.TaskContext.Interfaces;

public interface ITaskRepository : IRepository<TaskItem, Guid>
{
    Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetAllAsync(
        TaskItemStatus? statusFilter = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default);
}