using TaskFlow.Application.SharedContext.Abstractions;
using TaskFlow.Application.SharedContext.Responses;
using TaskFlow.Application.TaskContext.Interfaces;
using TaskFlow.Application.TaskContext.Responses;
using TaskFlow.Domain.TaskContext.Entities;

namespace TaskFlow.Application.TaskContext.Queries.GetAll;

public sealed class Handler(ITaskRepository repository)
    : IQueryHandler<Query, PagedResult<TaskResponse>>
{
    public async Task<PagedResult<TaskResponse>> HandleAsync(Query query, CancellationToken ct = default)
    {
        var (items, totalCount) = await repository.GetAllAsync(
            query.StatusFilter, query.Page, query.PageSize, ct);

        var responses = items.Select(MapToResponse).ToList().AsReadOnly();
        return new PagedResult<TaskResponse>(responses, query.Page, query.PageSize, totalCount);
    }

    private static TaskResponse MapToResponse(TaskItem task) => new()
    {
        Id = task.Id, Title = task.Title, Description = task.Description,
        Status = task.Status, CreatedAtUtc = task.CreatedAtUtc, UpdatedAtUtc = task.UpdatedAtUtc
    };
}