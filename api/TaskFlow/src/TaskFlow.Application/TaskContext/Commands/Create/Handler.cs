using Microsoft.Extensions.Logging;
using TaskFlow.Application.SharedContext.Abstractions;
using TaskFlow.Application.TaskContext.Interfaces;
using TaskFlow.Application.TaskContext.Responses;
using TaskFlow.Domain.SharedContext.UseCases;
using TaskFlow.Domain.TaskContext.Entities;
using TaskFlow.Domain.TaskContext.ValueObjects;

namespace TaskFlow.Application.TaskContext.Commands.Create;

public sealed class Handler(
    ITaskRepository repository,
    ILogger<Handler> logger)
    : ICommandHandler<Command, Result<TaskResponse>>
{
    public async Task<Result<TaskResponse>> HandleAsync(Command command, CancellationToken ct = default)
    {
        var titleResult = TaskTitle.Create(command.Title);
        if (!titleResult.IsSuccess)
            return Result<TaskResponse>.ValidationError(titleResult.Error!);

        var task = TaskItem.Create(titleResult.Value!, command.Description);
        var created = await repository.AddAsync(task, ct);

        logger.LogInformation("Created task {TaskId} — {Title}", created.Id, created.Title);
        return Result<TaskResponse>.Success(MapToResponse(created));
    }

    private static TaskResponse MapToResponse(TaskItem task) => new()
    {
        Id = task.Id, Title = task.Title, Description = task.Description,
        Status = task.Status, CreatedAtUtc = task.CreatedAtUtc
    };
}