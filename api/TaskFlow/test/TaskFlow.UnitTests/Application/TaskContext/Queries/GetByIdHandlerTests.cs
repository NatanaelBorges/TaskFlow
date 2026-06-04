using NSubstitute;
using TaskFlow.Application.TaskContext.Interfaces;
using TaskFlow.Application.TaskContext.Queries.GetById;
using TaskFlow.Domain.TaskContext.Entities;
using TaskFlow.Domain.TaskContext.ValueObjects;

namespace TaskFlow.UnitTests.Application.TaskContext.Queries;

public sealed class GetByIdHandlerTests
{
    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly Handler _handler;

    public GetByIdHandlerTests()
    {
        _handler = new Handler(_repository);
    }

    [Fact]
    public async Task HandleAsync_ExistingTask_ReturnsMappedResponse()
    {
        var id = Guid.NewGuid();
        var task = TaskItem.Create(TaskTitle.FromTrustedSource("My Task"), "details");
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(task);

        var response = await _handler.HandleAsync(new Query(id));

        Assert.NotNull(response);
        Assert.Equal("My Task", response.Title);
        Assert.Equal("details", response.Description);
    }

    [Fact]
    public async Task HandleAsync_TaskNotFound_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((TaskItem?)null);

        var response = await _handler.HandleAsync(new Query(id));

        Assert.Null(response);
    }
}
