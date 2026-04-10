using FluentResults;

namespace AnthropicShared;

public interface IAntropicClient : IDisposable
{
    IEnumerable<AnthropicMessage> Context { get; }
    Task<Result<string>> SendMessage(string message, string? systemPrompt = null);
}
