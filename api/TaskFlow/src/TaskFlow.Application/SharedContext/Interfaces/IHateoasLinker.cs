using TaskFlow.Application.SharedContext.Responses;

namespace TaskFlow.Application.SharedContext.Interfaces;

public interface IHateoasLinker<T>
{
    IReadOnlyDictionary<string, LinkResponse> GetLinks(T resource, string resourceBaseUrl);
}