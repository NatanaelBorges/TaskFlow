using TaskFlow.Api.Controllers.v1;
using TaskFlow.Application.TaskContext.Responses;
using TaskFlow.Domain.TaskContext.Enums;

namespace TaskFlow.UnitTests.Api;

public sealed class TaskLinkerTests
{
    private readonly TaskLinker _linker = new();
    private const string Base = "http://localhost/api/v1/tasks";

    private static TaskResponse ActiveTask() => new()
    {
        Id = Guid.NewGuid(), Title = "Task", Description = "", Status = TaskItemStatus.Active,
        CreatedAtUtc = DateTime.UtcNow
    };

    private static TaskResponse CompletedTask() => new()
    {
        Id = Guid.NewGuid(), Title = "Task", Description = "", Status = TaskItemStatus.Completed,
        CreatedAtUtc = DateTime.UtcNow
    };

    [Fact]
    public void GetLinks_AlwaysIncludesSelfUpdateDelete()
    {
        var links = _linker.GetLinks(ActiveTask(), Base);

        Assert.True(links.ContainsKey("self"));
        Assert.True(links.ContainsKey("update"));
        Assert.True(links.ContainsKey("delete"));
    }

    [Fact]
    public void GetLinks_ActiveTask_IncludesCompleteNotReopen()
    {
        var links = _linker.GetLinks(ActiveTask(), Base);

        Assert.True(links.ContainsKey("complete"));
        Assert.False(links.ContainsKey("reopen"));
    }

    [Fact]
    public void GetLinks_CompletedTask_IncludesReopenNotComplete()
    {
        var links = _linker.GetLinks(CompletedTask(), Base);

        Assert.True(links.ContainsKey("reopen"));
        Assert.False(links.ContainsKey("complete"));
    }

    [Fact]
    public void GetLinks_SelfAndCrudLinksPointToResourceUrl()
    {
        var task = ActiveTask();
        var links = _linker.GetLinks(task, Base);

        Assert.Contains(task.Id.ToString(), links["self"].Href);
        Assert.Contains(task.Id.ToString(), links["update"].Href);
        Assert.Contains(task.Id.ToString(), links["delete"].Href);
    }

    [Fact]
    public void GetLinks_StatusLink_PointsToStatusSubresource()
    {
        var task = ActiveTask();
        var links = _linker.GetLinks(task, Base);

        Assert.Contains("/status", links["complete"].Href);
        Assert.Equal("PATCH", links["complete"].Method);
    }

    [Fact]
    public void GetLinks_SelfUsesGet_UpdateUsesPut_DeleteUsesDelete()
    {
        var links = _linker.GetLinks(ActiveTask(), Base);

        Assert.Equal("GET", links["self"].Method);
        Assert.Equal("PUT", links["update"].Method);
        Assert.Equal("DELETE", links["delete"].Method);
    }
}