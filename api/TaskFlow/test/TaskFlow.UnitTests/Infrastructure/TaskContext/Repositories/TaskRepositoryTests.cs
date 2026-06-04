using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.TaskContext.Entities;
using TaskFlow.Domain.TaskContext.Enums;
using TaskFlow.Domain.TaskContext.ValueObjects;
using TaskFlow.Infrastructure.SharedContext.Data;
using TaskFlow.Infrastructure.TaskContext.Repositories;

namespace TaskFlow.UnitTests.Infrastructure.TaskContext.Repositories;

public sealed class TaskRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TaskRepository _repository;

    public TaskRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new TaskRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task AddAsync_PersistsEntity()
    {
        var task = NewTask("Task A");

        await _repository.AddAsync(task);

        Assert.Equal(1, await _context.Tasks.CountAsync());
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTask_ReturnsIt()
    {
        var task = await _repository.AddAsync(NewTask("Find me"));

        var found = await _repository.GetByIdAsync(task.Id);

        Assert.NotNull(found);
        Assert.Equal(task.Id, found.Id);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var found = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(found);
    }

    [Fact]
    public async Task GetByIdAsync_SoftDeletedTask_ReturnsNull()
    {
        var task = await _repository.AddAsync(NewTask("Ghost"));
        task.Delete();
        await _repository.UpdateAsync(task);

        var found = await _repository.GetByIdAsync(task.Id);

        Assert.Null(found);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedResults()
    {
        await SeedTasks(5);

        var (items, total) = await _repository.GetAllAsync(page: 1, pageSize: 3);

        Assert.Equal(3, items.Count);
        Assert.Equal(5, total);
    }

    [Fact]
    public async Task GetAllAsync_SecondPage_ReturnsRemainingItems()
    {
        await SeedTasks(5);

        var (items, _) = await _repository.GetAllAsync(page: 2, pageSize: 3);

        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task GetAllAsync_ExcludesSoftDeletedTasks()
    {
        var kept = await _repository.AddAsync(NewTask("Keep"));
        var deleted = await _repository.AddAsync(NewTask("Delete me"));
        deleted.Delete();
        await _repository.UpdateAsync(deleted);

        var (items, total) = await _repository.GetAllAsync();

        Assert.Equal(1, total);
        Assert.All(items, t => Assert.Equal(kept.Id, t.Id));
    }

    [Fact]
    public async Task GetAllAsync_FilterByActive_ReturnsOnlyActiveTasks()
    {
        var active = await _repository.AddAsync(NewTask("Active one"));
        var completed = await _repository.AddAsync(NewTask("Done one"));
        completed.Complete();
        await _repository.UpdateAsync(completed);

        var (items, total) = await _repository.GetAllAsync(statusFilter: TaskItemStatus.Active);

        Assert.Equal(1, total);
        Assert.Equal(active.Id, items[0].Id);
    }

    [Fact]
    public async Task GetAllAsync_FilterByCompleted_ReturnsOnlyCompletedTasks()
    {
        await _repository.AddAsync(NewTask("Still active"));
        var completed = await _repository.AddAsync(NewTask("Done"));
        completed.Complete();
        await _repository.UpdateAsync(completed);

        var (items, total) = await _repository.GetAllAsync(statusFilter: TaskItemStatus.Completed);

        Assert.Equal(1, total);
        Assert.Equal(TaskItemStatus.Completed, items[0].Status);
    }

    [Fact]
    public async Task GetAllAsync_NoFilter_ReturnsAllNonDeleted()
    {
        await _repository.AddAsync(NewTask("A"));
        var completed = await _repository.AddAsync(NewTask("B"));
        completed.Complete();
        await _repository.UpdateAsync(completed);

        var (_, total) = await _repository.GetAllAsync();

        Assert.Equal(2, total);
    }
    

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var task = await _repository.AddAsync(NewTask("Original"));
        task.Update(TaskTitle.FromTrustedSource("Updated"), "new desc", TaskItemStatus.Completed);

        await _repository.UpdateAsync(task);

        var fetched = await _repository.GetByIdAsync(task.Id);
        Assert.Equal("Updated", fetched!.Title.Value);
        Assert.Equal(TaskItemStatus.Completed, fetched.Status);
    }

    // --- Helpers ---

    private static TaskItem NewTask(string title) =>
        TaskItem.Create(TaskTitle.FromTrustedSource(title), "description");

    private async Task SeedTasks(int count)
    {
        for (var i = 0; i < count; i++)
            await _repository.AddAsync(NewTask($"Task {i + 1}"));
    }
}
