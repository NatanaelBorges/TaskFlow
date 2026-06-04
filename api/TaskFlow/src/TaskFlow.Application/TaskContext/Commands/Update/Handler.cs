using Microsoft.Extensions.Logging;
using TaskFlow.Application.SharedContext.Abstractions;
using TaskFlow.Application.TaskContext.Interfaces;
using TaskFlow.Application.TaskContext.Responses;
using TaskFlow.Domain.SharedContext.UseCases;
using TaskFlow.Domain.TaskContext.Entities;
using TaskFlow.Domain.TaskContext.ValueObjects;

namespace TaskFlow.Application.TaskContext.Commands.Update;

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

        var task = await repository.GetByIdAsync(command.Id, ct);
        if (task is null)
        {
            logger.LogWarning("Update failed: task {TaskId} not found", command.Id);
            return Result<TaskResponse>.NotFound($"Task '{command.Id}' was not found.");
        }

        task.Update(titleResult.Value!, command.Description, command.Status);
        await repository.UpdateAsync(task, ct);
        return Result<TaskResponse>.Success(MapToResponse(task));
    }

    private static TaskResponse MapToResponse(TaskItem task) => new()
    {
        Id = task.Id, Title = task.Title, Description = task.Description,
        Status = task.Status, CreatedAtUtc = task.CreatedAtUtc, UpdatedAtUtc = task.UpdatedAtUtc
    };
}