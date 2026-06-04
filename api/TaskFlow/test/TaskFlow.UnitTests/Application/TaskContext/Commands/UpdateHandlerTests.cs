using Microsoft.Extensions.Logging;
using NSubstitute;
using TaskFlow.Application.TaskContext.Commands.Update;
using TaskFlow.Application.TaskContext.Interfaces;
using TaskFlow.Domain.SharedContext.UseCases;
using TaskFlow.Domain.TaskContext.Entities;
using TaskFlow.Domain.TaskContext.Enums;
using TaskFlow.Domain.TaskContext.ValueObjects;

namespace TaskFlow.UnitTests.Application.TaskContext.Commands;

public sealed class UpdateHandlerTests
{
    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly ILogger<Handler> _logger = Substitute.For<ILogger<Handler>>();
    private readonly Handler _handler;

    public UpdateHandlerTests()
    {
        _handler = new Handler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_ExistingTask_ReturnsSuccessWithUpdatedTitleAndDescription()
    {
        var id = Guid.NewGuid();
        var existing = TaskItem.Create(TaskTitle.FromTrustedSource("Old Title"), "old desc");
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(existing);
        _repository.UpdateAsync(existing, Arg.Any<CancellationToken>()).Returns(existing);

        var result = await _handler.HandleAsync(new Command(id, "New Title", "new desc"));

        Assert.True(result.IsSuccess);
        Assert.Equal("New Title", result.Value!.Title);
        Assert.Equal("new desc", result.Value.Description);
    }

    [Fact]
    public async Task HandleAsync_ExistingTask_DoesNotChangeStatus()
    {
        var id = Guid.NewGuid();
        var existing = TaskItem.Create(TaskTitle.FromTrustedSource("Title"), "desc");
        existing.Complete();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(existing);
        _repository.UpdateAsync(existing, Arg.Any<CancellationToken>()).Returns(existing);

        var result = await _handler.HandleAsync(new Command(id, "New Title", "desc"));

        Assert.True(result.IsSuccess);
        Assert.Equal(TaskItemStatus.Completed, result.Value!.Status);
    }

    [Fact]
    public async Task HandleAsync_ExistingTask_PersistsUpdate()
    {
        var id = Guid.NewGuid();
        var existing = TaskItem.Create(TaskTitle.FromTrustedSource("Title"), "desc");
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(existing);
        _repository.UpdateAsync(existing, Arg.Any<CancellationToken>()).Returns(existing);

        await _handler.HandleAsync(new Command(id, "New Title", "desc"));

        await _repository.Received(1).UpdateAsync(existing, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_TaskNotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((TaskItem?)null);

        var result = await _handler.HandleAsync(new Command(id, "Title", "desc"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_TaskNotFound_DoesNotCallUpdate()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((TaskItem?)null);

        await _handler.HandleAsync(new Command(id, "Title", "desc"));

        await _repository.DidNotReceive().UpdateAsync(Arg.Any<TaskItem>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_InvalidTitle_ReturnsValidationError(string title)
    {
        var result = await _handler.HandleAsync(new Command(Guid.NewGuid(), title, "desc"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_InvalidTitle_DoesNotQueryRepository()
    {
        await _handler.HandleAsync(new Command(Guid.NewGuid(), "", "desc"));

        await _repository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
