using System.Text.Json.Serialization;
using TaskFlow.Application.SharedContext.Responses;
using TaskFlow.Domain.TaskContext.Enums;

namespace TaskFlow.Application.TaskContext.Responses;

public sealed record TaskResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public TaskItemStatus Status { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? UpdatedAtUtc { get; init; }

    [JsonPropertyName("_links")]
    public IReadOnlyDictionary<string, LinkResponse>? Links { get; init; }

    public TaskResponse WithLinks(IReadOnlyDictionary<string, LinkResponse> links) =>
        this with { Links = links };
}