using TaskFlow.Domain.SharedContext.Entities;
using TaskFlow.Domain.TaskContext.Enums;
using TaskFlow.Domain.TaskContext.ValueObjects;

namespace TaskFlow.Domain.TaskContext.Entities;

public sealed class TaskItem : Entity
{
    private TaskItem() { }

    public TaskTitle Title { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public TaskItemStatus Status { get; private set; }

    public static TaskItem Create(TaskTitle title, string description) => new()
    {
        Title = title,
        Description = description.Trim(),
        Status = TaskItemStatus.Active
    };

    public void Update(TaskTitle title, string description, TaskItemStatus status)
    {
        Title = title;
        Description = description.Trim();
        Status = status;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status == TaskItemStatus.Completed) return;
        Status = TaskItemStatus.Completed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Reopen()
    {
        if (Status == TaskItemStatus.Active) return;
        Status = TaskItemStatus.Active;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Delete() => SoftDelete();
}
