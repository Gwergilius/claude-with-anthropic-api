using FluentResults;

namespace AnthropicApiClient;

public interface IAntropicClient : IDisposable
{
    IEnumerable<AnthropicMessage> Context { get; }
    Task<Result<string>> SendMessage(string message);
}
