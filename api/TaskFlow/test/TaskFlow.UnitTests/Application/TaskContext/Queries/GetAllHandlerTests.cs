using NSubstitute;
using TaskFlow.Application.SharedContext.Responses;
using TaskFlow.Application.TaskContext.Interfaces;
using TaskFlow.Application.TaskContext.Queries.GetAll;
using TaskFlow.Application.TaskContext.Responses;
using TaskFlow.Domain.TaskContext.Entities;
using TaskFlow.Domain.TaskContext.Enums;
using TaskFlow.Domain.TaskContext.ValueObjects;

namespace TaskFlow.UnitTests.Application.TaskContext.Queries;

public sealed class GetAllHandlerTests
{
    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly Handler _handler;

    public GetAllHandlerTests()
    {
        _handler = new Handler(_repository);
    }

    [Fact]
    public async Task HandleAsync_WithTasks_ReturnsMappedPagedResult()
    {
        var tasks = new List<TaskItem>
        {
            TaskItem.Create(TaskTitle.FromTrustedSource("Task 1"), "desc 1"),
            TaskItem.Create(TaskTitle.FromTrustedSource("Task 2"), "desc 2"),
        };
        _repository
            .GetAllAsync(null, 1, 10, Arg.Any<CancellationToken>())
            .Returns(((IReadOnlyList<TaskItem>)tasks.AsReadOnly(), tasks.Count));

        var result = await _handler.HandleAsync(new Query(null, 1, 10));

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task HandleAsync_WithStatusFilter_PassesFilterToRepository()
    {
        _repository
            .GetAllAsync(TaskItemStatus.Active, 1, 10, Arg.Any<CancellationToken>())
            .Returns(((IReadOnlyList<TaskItem>)Array.Empty<TaskItem>(), 0));

        await _handler.HandleAsync(new Query(TaskItemStatus.Active, 1, 10));

        await _repository.Received(1)
            .GetAllAsync(TaskItemStatus.Active, 1, 10, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_EmptyRepository_ReturnsEmptyPagedResult()
    {
        _repository
            .GetAllAsync(null, 1, 10, Arg.Any<CancellationToken>())
            .Returns(((IReadOnlyList<TaskItem>)Array.Empty<TaskItem>(), 0));

        var result = await _handler.HandleAsync(new Query(null, 1, 10));

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
    }

    [Fact]
    public async Task HandleAsync_MapsTitlesCorrectly()
    {
        var tasks = new List<TaskItem>
        {
            TaskItem.Create(TaskTitle.FromTrustedSource("Alpha"), ""),
        };
        _repository
            .GetAllAsync(null, 1, 10, Arg.Any<CancellationToken>())
            .Returns(((IReadOnlyList<TaskItem>)tasks.AsReadOnly(), 1));

        var result = await _handler.HandleAsync(new Query(null, 1, 10));

        Assert.Equal("Alpha", result.Items[0].Title);
    }
}

public sealed class PagedResultTests
{
    [Theory]
    [InlineData(0, 10, 1)]
    [InlineData(10, 10, 1)]
    [InlineData(11, 10, 2)]
    [InlineData(20, 10, 2)]
    [InlineData(21, 10, 3)]
    public void TotalPages_ComputedCorrectly(int totalItems, int pageSize, int expectedPages)
    {
        var result = new PagedResult<TaskResponse>(Array.Empty<TaskResponse>(), 1, pageSize, totalItems);

        Assert.Equal(expectedPages, result.TotalPages);
    }

    [Fact]
    public void HasNextPage_WhenNotOnLastPage_IsTrue()
    {
        var result = new PagedResult<TaskResponse>(Array.Empty<TaskResponse>(), 1, 10, 25);

        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void HasNextPage_WhenOnLastPage_IsFalse()
    {
        var result = new PagedResult<TaskResponse>(Array.Empty<TaskResponse>(), 3, 10, 25);

        Assert.False(result.HasNextPage);
    }

    [Fact]
    public void HasPreviousPage_WhenOnFirstPage_IsFalse()
    {
        var result = new PagedResult<TaskResponse>(Array.Empty<TaskResponse>(), 1, 10, 25);

        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public void HasPreviousPage_WhenNotOnFirstPage_IsTrue()
    {
        var result = new PagedResult<TaskResponse>(Array.Empty<TaskResponse>(), 2, 10, 25);

        Assert.True(result.HasPreviousPage);
    }
}
