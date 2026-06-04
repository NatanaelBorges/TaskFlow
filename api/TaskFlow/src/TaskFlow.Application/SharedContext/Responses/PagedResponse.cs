using System.Text.Json.Serialization;

namespace TaskFlow.Application.SharedContext.Responses;

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Data,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages,
    [property: JsonPropertyName("_links")] IReadOnlyDictionary<string, LinkResponse?> Links
);