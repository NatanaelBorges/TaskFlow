using TaskFlow.Domain.TaskContext.Enums;

namespace TaskFlow.Application.TaskContext.Commands.Update;

public sealed record Command(Guid Id, string Title, string Description, TaskItemStatus Status);