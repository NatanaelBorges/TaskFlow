using TaskFlow.Application.SharedContext.Responses;

namespace TaskFlow.Api.Helpers;

public static class PaginationLinksHelper
{
    public static IReadOnlyDictionary<string, LinkResponse?> Generate(
        string collectionBaseUrl,
        int page,
        int pageSize,
        int totalPages,
        string? statusFilter = null)
    {
        string Url(int p) =>
            $"{collectionBaseUrl}?page={p}&pageSize={pageSize}" +
            (statusFilter is not null ? $"&status={statusFilter}" : string.Empty);

        return new Dictionary<string, LinkResponse?>
        {
            ["self"]  = new(Url(page), "GET"),
            ["first"] = new(Url(1), "GET"),
            ["last"]  = new(Url(totalPages), "GET"),
            ["next"]  = page < totalPages ? new(Url(page + 1), "GET") : null,
            ["prev"]  = page > 1 ? new(Url(page - 1), "GET") : null,
        };
    }
}