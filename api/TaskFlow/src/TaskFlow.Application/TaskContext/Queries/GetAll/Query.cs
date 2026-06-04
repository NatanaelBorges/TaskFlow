using TaskFlow.Domain.TaskContext.Enums;

namespace TaskFlow.Application.TaskContext.Queries.GetAll;

public sealed record Query(
    TaskItemStatus? StatusFilter,
    int Page,
    int PageSize);