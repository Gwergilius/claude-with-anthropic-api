using System.Text.Json;

namespace AnthropicApiClient;

public interface IAntropicClient : IDisposable
{
    Task<JsonDocument> SendMessage(string message);
}