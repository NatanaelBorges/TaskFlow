using Microsoft.Extensions.Logging;
using NSubstitute;
using TaskFlow.Application.TaskContext.Commands.Delete;
using TaskFlow.Application.TaskContext.Interfaces;
using TaskFlow.Domain.SharedContext.UseCases;
using TaskFlow.Domain.TaskContext.Entities;
using TaskFlow.Domain.TaskContext.ValueObjects;

namespace TaskFlow.UnitTests.Application.TaskContext.Commands;

public sealed class DeleteHandlerTests
{
    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly ILogger<Handler> _logger = Substitute.For<ILogger<Handler>>();
    private readonly Handler _handler;

    public DeleteHandlerTests()
    {
        _handler = new Handler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_ExistingTask_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        var task = TaskItem.Create(TaskTitle.FromTrustedSource("Task"), "desc");
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(task);
        _repository.UpdateAsync(task, Arg.Any<CancellationToken>()).Returns(task);

        var result = await _handler.HandleAsync(new Command(id));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task HandleAsync_ExistingTask_SoftDeletesViaUpdate()
    {
        var id = Guid.NewGuid();
        var task = TaskItem.Create(TaskTitle.FromTrustedSource("Task"), "desc");
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(task);
        _repository.UpdateAsync(Arg.Any<TaskItem>(), Arg.Any<CancellationToken>()).Returns(task);

        await _handler.HandleAsync(new Command(id));

        await _repository.Received(1).UpdateAsync(
            Arg.Is<TaskItem>(t => t.IsDeleted),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_TaskNotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((TaskItem?)null);

        var result = await _handler.HandleAsync(new Command(id));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_TaskNotFound_DoesNotCallUpdate()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((TaskItem?)null);

        await _handler.HandleAsync(new Command(id));

        await _repository.DidNotReceive().UpdateAsync(Arg.Any<TaskItem>(), Arg.Any<CancellationToken>());
    }
}
