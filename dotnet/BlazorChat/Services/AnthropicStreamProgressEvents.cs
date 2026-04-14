namespace BlazorChat.Services;

public sealed class AnthropicStreamDeltaEventArgs(string deltaText) : EventArgs
{
    public string DeltaText { get; } = deltaText;
}

public enum AnthropicStreamCompletionReason
{
    Completed,
    Faulted,
    Cancelled
}

public sealed class AnthropicStreamCompletedEventArgs : EventArgs
{
    public required AnthropicStreamCompletionReason Reason { get; init; }
    public string? ErrorMessage { get; init; }
    public Exception? Exception { get; init; }
}
