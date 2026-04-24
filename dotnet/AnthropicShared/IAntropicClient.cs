
namespace AnthropicShared;

public interface IAntropicClient : IDisposable
{
    IEnumerable<AnthropicMessage> Context { get; }
    Task<Result<string>> SendMessage(string message, string? systemPrompt = null, double? temperatureOverride = null);

    /// <summary>
    /// Sends a streaming request (SSE). On HTTP success, appends an empty assistant message to context
    /// and returns the response body stream. Caller must dispose the returned value after reading completes.
    /// </summary>
    Task<Result<AnthropicStreamingResponse>> StartStreamingMessageAsync(
        string message,
        string? systemPrompt = null,
        CancellationToken cancellationToken = default);

    /// <summary>Appends text to the last assistant message in context (streaming deltas).</summary>
    void AppendLastAssistantMessageText(string delta);
}
