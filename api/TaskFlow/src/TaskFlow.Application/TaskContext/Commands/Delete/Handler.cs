using Microsoft.Extensions.Logging;
using TaskFlow.Application.SharedContext.Abstractions;
using TaskFlow.Application.TaskContext.Interfaces;
using TaskFlow.Domain.SharedContext.UseCases;

namespace TaskFlow.Application.TaskContext.Commands.Delete;

public sealed class Handler(
    ITaskRepository repository,
    ILogger<Handler> logger)
    : ICommandHandler<Command, Result<bool>>
{
    public async Task<Result<bool>> HandleAsync(Command command, CancellationToken ct = default)
    {
        
        var task = await repository.GetByIdAsync(command.Id, ct);
        if (task is null)
        {
            logger.LogWarning("Delete failed: task {TaskId} not found", command.Id);
            return Result<bool>.NotFound($"Task '{command.Id}' was not found.");
        }

        task.Delete();
        await repository.UpdateAsync(task, ct);

        logger.LogInformation("Deleted task {TaskId}", command.Id);
        return Result<bool>.Success(true);
    }
}