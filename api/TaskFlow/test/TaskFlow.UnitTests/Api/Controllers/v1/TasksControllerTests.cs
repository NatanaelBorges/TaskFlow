using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using TaskFlow.Api.Controllers.v1;
using TaskFlow.Application.SharedContext.Abstractions;
using TaskFlow.Application.SharedContext.Interfaces;
using TaskFlow.Application.SharedContext.Responses;
using TaskFlow.Application.TaskContext.Responses;
using TaskFlow.Domain.SharedContext.UseCases;
using TaskFlow.Domain.TaskContext.Enums;
using GetAll = TaskFlow.Application.TaskContext.Queries.GetAll;
using GetById = TaskFlow.Application.TaskContext.Queries.GetById;
using Create = TaskFlow.Application.TaskContext.Commands.Create;
using Update = TaskFlow.Application.TaskContext.Commands.Update;
using Patch = TaskFlow.Application.TaskContext.Commands.Patch;
using Delete = TaskFlow.Application.TaskContext.Commands.Delete;

namespace TaskFlow.UnitTests.Api.Controllers.v1;

public sealed class TasksControllerTests
{
    private readonly IQueryHandler<GetAll.Query, PagedResult<TaskResponse>> _getAll
        = Substitute.For<IQueryHandler<GetAll.Query, PagedResult<TaskResponse>>>();
    private readonly IQueryHandler<GetById.Query, TaskResponse?> _getById
        = Substitute.For<IQueryHandler<GetById.Query, TaskResponse?>>();
    private readonly ICommandHandler<Create.Command, Result<TaskResponse>> _create
        = Substitute.For<ICommandHandler<Create.Command, Result<TaskResponse>>>();
    private readonly ICommandHandler<Update.Command, Result<TaskResponse>> _update
        = Substitute.For<ICommandHandler<Update.Command, Result<TaskResponse>>>();
    private readonly ICommandHandler<Patch.Command, Result<TaskResponse>> _patch
        = Substitute.For<ICommandHandler<Patch.Command, Result<TaskResponse>>>();
    private readonly ICommandHandler<Delete.Command, Result<bool>> _delete
        = Substitute.For<ICommandHandler<Delete.Command, Result<bool>>>();
    private readonly IHateoasLinker<TaskResponse> _linker
        = Substitute.For<IHateoasLinker<TaskResponse>>();

    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _controller = new TasksController(_getAll, _getById, _create, _update, _patch, _delete, _linker);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.ControllerContext.HttpContext.Request.Scheme = "http";
        _controller.ControllerContext.HttpContext.Request.Host = new HostString("localhost");

        _linker.GetLinks(Arg.Any<TaskResponse>(), Arg.Any<string>())
            .Returns(new Dictionary<string, LinkResponse>());
    }
    
    [Theory]
    [InlineData("active",    TaskItemStatus.Active)]
    [InlineData("ACTIVE",    TaskItemStatus.Active)]
    [InlineData("Active",    TaskItemStatus.Active)]
    [InlineData("completed", TaskItemStatus.Completed)]
    [InlineData("COMPLETED", TaskItemStatus.Completed)]
    public async Task GetAll_KnownStatusString_PassesCorrectFilterToHandler(
        string statusParam, TaskItemStatus expectedEnum)
    {
        SetupGetAllReturnsEmpty();

        await _controller.GetAll(statusParam, 1, 10, CancellationToken.None);

        await _getAll.Received(1).HandleAsync(
            Arg.Is<GetAll.Query>(q => q.StatusFilter == expectedEnum),
            Arg.Any<CancellationToken>());
    }
    
    [Theory]
    [InlineData("all")]
    [InlineData("unknown")]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetAll_UnknownOrNullStatus_PassesNullFilterToHandler(string? statusParam)
    {
        SetupGetAllReturnsEmpty();

        await _controller.GetAll(statusParam, 1, 10, CancellationToken.None);

        await _getAll.Received(1).HandleAsync(
            Arg.Is<GetAll.Query>(q => q.StatusFilter == null),
            Arg.Any<CancellationToken>());
    }
    
    [Theory]
    [InlineData(0,   1)]
    [InlineData(-5,  1)]
    [InlineData(1,   1)]
    [InlineData(10, 10)]
    public async Task GetAll_PageBelowOne_IsClampedToOne(int inputPage, int expectedPage)
    {
        SetupGetAllReturnsEmpty();

        await _controller.GetAll(null, inputPage, 10, CancellationToken.None);

        await _getAll.Received(1).HandleAsync(
            Arg.Is<GetAll.Query>(q => q.Page == expectedPage),
            Arg.Any<CancellationToken>());
    }
    
    
    [Theory]
    [InlineData(0,   1)]
    [InlineData(101, 100)]
    [InlineData(50,  50)]
    public async Task GetAll_PageSizeOutOfRange_IsClamped(int inputSize, int expectedSize)
    {
        SetupGetAllReturnsEmpty();

        await _controller.GetAll(null, 1, inputSize, CancellationToken.None);

        await _getAll.Received(1).HandleAsync(
            Arg.Is<GetAll.Query>(q => q.PageSize == expectedSize),
            Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Update_NotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _update.HandleAsync(Arg.Any<Update.Command>(), Arg.Any<CancellationToken>())
            .Returns(Result<TaskResponse>.NotFound("not found"));

        var result = await _controller.Update(id, new Update.Request { Title = "T", Description = "" }, CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problem.StatusCode);
    }

    [Fact]
    public async Task Update_ValidationError_Returns400()
    {
        var id = Guid.NewGuid();
        _update.HandleAsync(Arg.Any<Update.Command>(), Arg.Any<CancellationToken>())
            .Returns(Result<TaskResponse>.ValidationError("bad input"));

        var result = await _controller.Update(id, new Update.Request { Title = "T", Description = "" }, CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
    }

    [Fact]
    public async Task Update_UnexpectedError_Returns500()
    {
        var id = Guid.NewGuid();
        _update.HandleAsync(Arg.Any<Update.Command>(), Arg.Any<CancellationToken>())
            .Returns(Result<TaskResponse>.Unexpected("boom"));

        var result = await _controller.Update(id, new Update.Request { Title = "T", Description = "" }, CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, problem.StatusCode);
    }

    [Fact]
    public async Task PatchStatus_NotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _patch.HandleAsync(Arg.Any<Patch.Command>(), Arg.Any<CancellationToken>())
            .Returns(Result<TaskResponse>.NotFound("not found"));

        var result = await _controller.PatchStatus(id, CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problem.StatusCode);
    }
    
    [Fact]
    public async Task GetById_TaskExists_Returns200WithLinkedResponse()
    {
        var id = Guid.NewGuid();
        var response = MakeTaskResponse(id, TaskItemStatus.Active);
        _getById.HandleAsync(Arg.Is<GetById.Query>(q => q.Id == id), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _controller.GetById(id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
    }

    [Fact]
    public async Task GetById_TaskNotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _getById.HandleAsync(Arg.Any<GetById.Query>(), Arg.Any<CancellationToken>())
            .Returns((TaskResponse?)null);

        var result = await _controller.GetById(id, CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problem.StatusCode);
    }

    [Fact]
    public async Task Create_ValidRequest_Returns201CreatedAtAction()
    {
        var response = MakeTaskResponse(Guid.NewGuid(), TaskItemStatus.Active);
        _create.HandleAsync(Arg.Any<Create.Command>(), Arg.Any<CancellationToken>())
            .Returns(Result<TaskResponse>.Success(response));

        var result = await _controller.Create(
            new Create.Request { Title = "New Task", Description = "desc" },
            CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
    }

    [Fact]
    public async Task Create_HandlerReturnsValidationError_Returns400()
    {
        _create.HandleAsync(Arg.Any<Create.Command>(), Arg.Any<CancellationToken>())
            .Returns(Result<TaskResponse>.ValidationError("Title is required."));

        var result = await _controller.Create(
            new Create.Request { Title = "T", Description = "" },
            CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
    }


    [Fact]
    public async Task Delete_TaskExists_Returns204NoContent()
    {
        var id = Guid.NewGuid();
        _delete.HandleAsync(Arg.Any<Delete.Command>(), Arg.Any<CancellationToken>())
            .Returns(Result<bool>.Success(true));

        var result = await _controller.Delete(id, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_TaskNotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _delete.HandleAsync(Arg.Any<Delete.Command>(), Arg.Any<CancellationToken>())
            .Returns(Result<bool>.NotFound($"Task '{id}' was not found."));

        var result = await _controller.Delete(id, CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problem.StatusCode);
    }
    
    // --- Helpers 

    private void SetupGetAllReturnsEmpty()
    {
        _getAll.HandleAsync(Arg.Any<GetAll.Query>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<TaskResponse>(Array.Empty<TaskResponse>(), 1, 10, 0));
    }
    
    private static TaskResponse MakeTaskResponse(Guid id, TaskItemStatus status) => new()
    {
        Id = id, Title = "Task", Description = "", Status = status,
        CreatedAtUtc = DateTime.UtcNow
    };
}