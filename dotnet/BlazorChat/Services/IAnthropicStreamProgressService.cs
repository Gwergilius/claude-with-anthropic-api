namespace BlazorChat.Services;

/// <summary>
/// Reads an Anthropic Messages SSE stream on a background task and surfaces progress events
/// (similar in spirit to BackgroundWorker: Start / Cancel / progress callbacks).
/// </summary>
public interface IAnthropicStreamProgressService
{
    event EventHandler? ProgressStarted;
    event EventHandler<AnthropicStreamDeltaEventArgs>? ProgressChanged;
    event EventHandler<AnthropicStreamCompletedEventArgs>? ProgressCompleted;

    /// <summary>Begins reading <paramref name="stream"/> until completion, cancellation, or error.</summary>
    void Start(Stream stream, CancellationToken cancellationToken = default);

    /// <summary>Requests cooperative cancellation of the in-flight read loop.</summary>
    void Cancel();
}
