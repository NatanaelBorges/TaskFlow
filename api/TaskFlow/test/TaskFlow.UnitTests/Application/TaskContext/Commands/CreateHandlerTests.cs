using Microsoft.Extensions.Logging;
using NSubstitute;
using TaskFlow.Application.TaskContext.Commands.Create;
using TaskFlow.Application.TaskContext.Interfaces;
using TaskFlow.Domain.SharedContext.UseCases;
using TaskFlow.Domain.TaskContext.Entities;
using TaskFlow.Domain.TaskContext.Enums;
using TaskFlow.Domain.TaskContext.ValueObjects;

namespace TaskFlow.UnitTests.Application.TaskContext.Commands;

public sealed class CreateHandlerTests
{
    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly ILogger<Handler> _logger = Substitute.For<ILogger<Handler>>();
    private readonly Handler _handler;

    public CreateHandlerTests()
    {
        _handler = new Handler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsSuccessWithActiveTask()
    {
        var command = new Command("Buy groceries", "Get milk and bread");
        var stored = TaskItem.Create(TaskTitle.FromTrustedSource("Buy groceries"), "Get milk and bread");
        _repository.AddAsync(Arg.Any<TaskItem>(), Arg.Any<CancellationToken>()).Returns(stored);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Equal("Buy groceries", result.Value!.Title);
        Assert.Equal(TaskItemStatus.Active, result.Value.Status);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_PersistsTaskViaRepository()
    {
        var command = new Command("Task A", "desc");
        var stored = TaskItem.Create(TaskTitle.FromTrustedSource("Task A"), "desc");
        _repository.AddAsync(Arg.Any<TaskItem>(), Arg.Any<CancellationToken>()).Returns(stored);

        await _handler.HandleAsync(command);

        await _repository.Received(1).AddAsync(Arg.Any<TaskItem>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_EmptyTitle_ReturnsValidationError(string title)
    {
        var result = await _handler.HandleAsync(new Command(title, "desc"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_TitleExceeds200Chars_ReturnsValidationError()
    {
        var result = await _handler.HandleAsync(new Command(new string('x', 201), "desc"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_InvalidTitle_DoesNotCallRepository()
    {
        await _handler.HandleAsync(new Command("", "desc"));

        await _repository.DidNotReceive().AddAsync(Arg.Any<TaskItem>(), Arg.Any<CancellationToken>());
    }
}