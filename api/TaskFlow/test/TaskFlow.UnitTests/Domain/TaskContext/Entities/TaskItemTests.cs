using TaskFlow.Domain.TaskContext.Entities;
using TaskFlow.Domain.TaskContext.Enums;
using TaskFlow.Domain.TaskContext.ValueObjects;

namespace TaskFlow.UnitTests.Domain.TaskContext.Entities;

public sealed class TaskItemTests
{
    private static TaskTitle ValidTitle => TaskTitle.FromTrustedSource("Test Task");

    [Fact]
    public void Create_SetsActiveStatus()
    {
        var task = TaskItem.Create(ValidTitle, "description");

        Assert.Equal(TaskItemStatus.Active, task.Status);
    }

    [Fact]
    public void Create_AssignsNewId()
    {
        var task = TaskItem.Create(ValidTitle, "description");

        Assert.NotEqual(Guid.Empty, task.Id);
    }

    [Fact]
    public void Create_TrimsDescription()
    {
        var task = TaskItem.Create(ValidTitle, "  trimmed  ");

        Assert.Equal("trimmed", task.Description);
    }

    [Fact]
    public void Create_IsNotDeleted()
    {
        var task = TaskItem.Create(ValidTitle, "desc");

        Assert.False(task.IsDeleted);
        Assert.Null(task.DeletedAtUtc);
    }

    [Fact]
    public void Create_UpdatedAtUtcIsNull()
    {
        var task = TaskItem.Create(ValidTitle, "desc");

        Assert.Null(task.UpdatedAtUtc);
    }

    [Fact]
    public void Complete_ActiveTask_TransitionsToCompleted()
    {
        var task = TaskItem.Create(ValidTitle, "desc");

        task.Complete();

        Assert.Equal(TaskItemStatus.Completed, task.Status);
        Assert.NotNull(task.UpdatedAtUtc);
    }

    [Fact]
    public void Complete_AlreadyCompleted_IsIdempotent()
    {
        var task = TaskItem.Create(ValidTitle, "desc");
        task.Complete();
        var timestampAfterFirst = task.UpdatedAtUtc;

        task.Complete();

        Assert.Equal(timestampAfterFirst, task.UpdatedAtUtc);
    }

    [Fact]
    public void Reopen_CompletedTask_TransitionsToActive()
    {
        var task = TaskItem.Create(ValidTitle, "desc");
        task.Complete();

        task.Reopen();

        Assert.Equal(TaskItemStatus.Active, task.Status);
        Assert.NotNull(task.UpdatedAtUtc);
    }

    [Fact]
    public void Reopen_AlreadyActive_IsIdempotent()
    {
        var task = TaskItem.Create(ValidTitle, "desc");
        var initialTimestamp = task.UpdatedAtUtc;

        task.Reopen();

        Assert.Equal(initialTimestamp, task.UpdatedAtUtc);
    }

    [Fact]
    public void Delete_SetsIsDeletedAndTimestamp()
    {
        var task = TaskItem.Create(ValidTitle, "desc");

        task.Delete();

        Assert.True(task.IsDeleted);
        Assert.NotNull(task.DeletedAtUtc);
        Assert.NotNull(task.UpdatedAtUtc);
    }

    [Fact]
    public void Update_SetsNewValuesAndTimestamp()
    {
        var task = TaskItem.Create(ValidTitle, "old description");
        var newTitle = TaskTitle.FromTrustedSource("New Title");

        task.Update(newTitle, " updated desc ", TaskItemStatus.Completed);

        Assert.Equal("New Title", task.Title.Value);
        Assert.Equal("updated desc", task.Description);
        Assert.Equal(TaskItemStatus.Completed, task.Status);
        Assert.NotNull(task.UpdatedAtUtc);
    }

    [Fact]
    public void TwoTasks_HaveDifferentIds()
    {
        var task1 = TaskItem.Create(ValidTitle, "desc");
        var task2 = TaskItem.Create(ValidTitle, "desc");

        Assert.NotEqual(task1.Id, task2.Id);
    }

    [Fact]
    public void Equals_SameId_ReturnsTrue()
    {
        var task = TaskItem.Create(ValidTitle, "desc");

        Assert.True(task.Equals(task.Id));
    }

    [Fact]
    public void Equals_DifferentId_ReturnsFalse()
    {
        var task = TaskItem.Create(ValidTitle, "desc");

        Assert.False(task.Equals(Guid.NewGuid()));
    }
}
