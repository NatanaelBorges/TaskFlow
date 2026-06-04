using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Helpers;
using TaskFlow.Application.SharedContext.Abstractions;
using TaskFlow.Application.SharedContext.Interfaces;
using TaskFlow.Application.SharedContext.Responses;
using GetAll = TaskFlow.Application.TaskContext.Queries.GetAll;
using GetById = TaskFlow.Application.TaskContext.Queries.GetById;
using Create = TaskFlow.Application.TaskContext.Commands.Create;
using Update = TaskFlow.Application.TaskContext.Commands.Update;
using Patch = TaskFlow.Application.TaskContext.Commands.Patch;
using Delete = TaskFlow.Application.TaskContext.Commands.Delete;
using TaskFlow.Application.TaskContext.Responses;
using TaskFlow.Domain.SharedContext.UseCases;
using TaskFlow.Domain.TaskContext.Enums;

namespace TaskFlow.Api.Controllers.v1;

[ApiController]
[Route("api/v1/tasks")]
[Tags("V1 - Tasks")]
[Produces("application/json")]
public sealed class TasksController(
    IQueryHandler<GetAll.Query, PagedResult<TaskResponse>> getAll,
    IQueryHandler<GetById.Query, TaskResponse?> getById,
    ICommandHandler<Create.Command, Result<TaskResponse>> create,
    ICommandHandler<Update.Command, Result<TaskResponse>> update,
    ICommandHandler<Patch.Command, Result<TaskResponse>> patch,
    ICommandHandler<Delete.Command, Result<bool>> delete,
    IHateoasLinker<TaskResponse> linker
    ): ControllerBase
{
    private const int MaxPageSize = 100;
    private string BaseUrl => $"{Request.Scheme}://{Request.Host}/api/v1/tasks";
    
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<TaskResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        page     = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        TaskItemStatus? statusFilter = status?.ToLowerInvariant() switch
        {
            "active"    => TaskItemStatus.Active,
            "completed" => TaskItemStatus.Completed,
            _           => null
        };

        var result = await getAll.HandleAsync(new GetAll.Query(statusFilter, page, pageSize), ct);

        var data = result.Items
            .Select(t => t.WithLinks(linker.GetLinks(t, BaseUrl)))
            .ToList()
            .AsReadOnly();

        return Ok(new PagedResponse<TaskResponse>(
            data,
            result.Page,
            result.PageSize,
            result.TotalItems,
            result.TotalPages,
            PaginationLinksHelper.Generate(BaseUrl, page, pageSize, result.TotalPages, status)));
    }
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var task = await getById.HandleAsync(new GetById.Query(id), ct);
        if (task is null)
            return Problem(detail: $"Task '{id}' was not found.", statusCode: StatusCodes.Status404NotFound);

        return Ok(task.WithLinks(linker.GetLinks(task, BaseUrl)));
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] Create.Request request, CancellationToken ct)
    {
        var result = await create.HandleAsync(new Create.Command(request.Title, request.Description), ct);
        if (!result.IsSuccess)
            return Problem(detail: result.Error!, statusCode: StatusCodes.Status400BadRequest);

        var response = result.Value!.WithLinks(linker.GetLinks(result.Value, BaseUrl));
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }
    
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] Update.Request request, CancellationToken ct)
    {
        var result = await update.HandleAsync(
            new Update.Command(id, request.Title, request.Description, request.Status), ct);
        return ToActionResult(result);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PatchStatus(Guid id, CancellationToken ct)
    {
        var result = await patch.HandleAsync(new Patch.Command(id), ct);
        return ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await delete.HandleAsync(new Delete.Command(id), ct);
        if (!result.IsSuccess && result.ErrorType == ResultErrorType.NotFound)
            return Problem(detail: result.Error!, statusCode: StatusCodes.Status404NotFound);

        return NoContent();
    }

    private IActionResult ToActionResult(Result<TaskResponse> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value!.WithLinks(linker.GetLinks(result.Value, BaseUrl)));

        return result.ErrorType switch
        {
            ResultErrorType.NotFound   => Problem(detail: result.Error!, statusCode: StatusCodes.Status404NotFound),
            ResultErrorType.Validation => Problem(detail: result.Error!, statusCode: StatusCodes.Status400BadRequest),
            _                          => Problem(detail: result.Error!, statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}

#region Helpers

public sealed class TaskLinker : IHateoasLinker<TaskResponse>
{
    public IReadOnlyDictionary<string, LinkResponse> GetLinks(TaskResponse task, string resourceBaseUrl)
    {
        var links = new Dictionary<string, LinkResponse>
        {
            ["self"]   = new($"{resourceBaseUrl}/{task.Id}", "GET"),
            ["update"] = new($"{resourceBaseUrl}/{task.Id}", "PUT"),
            ["delete"] = new($"{resourceBaseUrl}/{task.Id}", "DELETE"),
        };

        links[task.Status == TaskItemStatus.Active ? "complete" : "reopen"] =
            new($"{resourceBaseUrl}/{task.Id}/status", "PATCH");

        return links;
    }
}

#endregion