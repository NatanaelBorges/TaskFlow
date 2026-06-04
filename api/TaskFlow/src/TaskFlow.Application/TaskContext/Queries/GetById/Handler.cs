using TaskFlow.Application.SharedContext.Abstractions;
using TaskFlow.Application.TaskContext.Interfaces;
using TaskFlow.Application.TaskContext.Responses;

namespace TaskFlow.Application.TaskContext.Queries.GetById;

public sealed class Handler(ITaskRepository repository)
    : IQueryHandler<Query, TaskResponse?>
{
    public async Task<TaskResponse?> HandleAsync(Query query, CancellationToken ct = default)
    {
        var task = await repository.GetByIdAsync(query.Id, ct);
        if (task is null) return null;

        return new TaskResponse
        {
            Id = task.Id, Title = task.Title, Description = task.Description,
            Status = task.Status, CreatedAtUtc = task.CreatedAtUtc, UpdatedAtUtc = task.UpdatedAtUtc
        };
    }
}