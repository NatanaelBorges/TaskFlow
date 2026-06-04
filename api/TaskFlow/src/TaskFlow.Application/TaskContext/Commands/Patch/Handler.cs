using Microsoft.Extensions.Logging;
using TaskFlow.Application.SharedContext.Abstractions;
using TaskFlow.Application.TaskContext.Interfaces;
using TaskFlow.Application.TaskContext.Responses;
using TaskFlow.Domain.SharedContext.UseCases;
using TaskFlow.Domain.TaskContext.Enums;

namespace TaskFlow.Application.TaskContext.Commands.Patch;

public sealed class Handler(
    ITaskRepository repository,
    ILogger<Handler> logger)
    : ICommandHandler<Command, Result<TaskResponse>>
{
    public async Task<Result<TaskResponse>> HandleAsync(Command command, CancellationToken ct = default)
    {
        var task = await repository.GetByIdAsync(command.Id, ct);
        if (task is null)
        {
            logger.LogWarning("Patch status failed: task {TaskId} not found", command.Id);
            return Result<TaskResponse>.NotFound($"Task '{command.Id}' was not found.");
        }

        if (task.Status == TaskItemStatus.Active) task.Complete();
        else task.Reopen();

        await repository.UpdateAsync(task, ct);

        return Result<TaskResponse>.Success(new TaskResponse
        {
            Id = task.Id, Title = task.Title, Description = task.Description,
            Status = task.Status, CreatedAtUtc = task.CreatedAtUtc, UpdatedAtUtc = task.UpdatedAtUtc
        });
    }
}