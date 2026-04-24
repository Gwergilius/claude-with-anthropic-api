
namespace BlazorChat.Services;

public sealed class AnthropicStreamProgressService(ILogger<AnthropicStreamProgressService> logger)
    : IAnthropicStreamProgressService
{
    private readonly object _gate = new();
    private CancellationTokenSource? _linkedCts;
    private int _completionSent;

    public event EventHandler? ProgressStarted;
    public event EventHandler<AnthropicStreamDeltaEventArgs>? ProgressChanged;
    public event EventHandler<AnthropicStreamCompletedEventArgs>? ProgressCompleted;

    public void Start(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        lock (_gate)
        {
            _linkedCts?.Cancel();
            _linkedCts?.Dispose();
            Interlocked.Exchange(ref _completionSent, 0);
            _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var readToken = _linkedCts.Token;
            _ = ReadStreamAsync(stream, readToken);
        }
    }

    public void Cancel()
    {
        lock (_gate)
        {
            _linkedCts?.Cancel();
        }
    }

    private async Task ReadStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        try
        {
            ProgressStarted?.Invoke(this, EventArgs.Empty);

            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

            string? sseEventName = null;
            var dataLines = new List<string>();

            while (!cancellationToken.IsCancellationRequested)
            {
                string? line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);

                if (line is null)
                {
                    if (sseEventName is not null || dataLines.Count > 0)
                    {
                        if (TryProcessSseBlock(sseEventName, dataLines) is { } earlyEnd)
                        {
                            RaiseCompletedOnce(earlyEnd);
                            return;
                        }
                    }

                    RaiseCompletedOnce(
                        new AnthropicStreamCompletedEventArgs
                        {
                            Reason = AnthropicStreamCompletionReason.Faulted,
                            ErrorMessage = "Stream ended before a terminal message_stop event."
                        });
                    return;
                }

                if (line.Length == 0)
                {
                    if (sseEventName is not null || dataLines.Count > 0)
                    {
                        if (TryProcessSseBlock(sseEventName, dataLines) is { } blockEnd)
                        {
                            RaiseCompletedOnce(blockEnd);
                            return;
                        }

                        sseEventName = null;
                        dataLines.Clear();
                    }

                    continue;
                }

                if (line[0] == ':')
                {
                    continue;
                }

                if (line.StartsWith("event:", StringComparison.Ordinal))
                {
                    sseEventName = line["event:".Length..].Trim();
                }
                else if (line.StartsWith("data:", StringComparison.Ordinal))
                {
                    dataLines.Add(line["data:".Length..].TrimStart());
                }
            }

            RaiseCompletedOnce(
                new AnthropicStreamCompletedEventArgs { Reason = AnthropicStreamCompletionReason.Cancelled });
        }
        catch (OperationCanceledException)
        {
            RaiseCompletedOnce(
                new AnthropicStreamCompletedEventArgs { Reason = AnthropicStreamCompletionReason.Cancelled });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Anthropic SSE read failed");
            RaiseCompletedOnce(
                new AnthropicStreamCompletedEventArgs
                {
                    Reason = AnthropicStreamCompletionReason.Faulted,
                    ErrorMessage = ex.Message,
                    Exception = ex
                });
        }
    }

    /// <summary>
    /// Logs one verbose-style entry per SSE event (maps to <see cref="LogLevel.Trace"/> in Microsoft.Extensions.Logging).
    /// </summary>
    private AnthropicStreamCompletedEventArgs? TryProcessSseBlock(string? sseEventName, List<string> dataLines)
    {
        var payload = dataLines.Count == 0 ? string.Empty : string.Join("\n", dataLines);
        logger.LogTrace("Anthropic SSE event: sseEvent={SseEvent}, data={Data}", sseEventName ?? "(none)", payload);

        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(payload);
        }
        catch (JsonException ex)
        {
            return new AnthropicStreamCompletedEventArgs
            {
                Reason = AnthropicStreamCompletionReason.Faulted,
                ErrorMessage = $"Invalid SSE JSON: {ex.Message}",
                Exception = ex
            };
        }

        using (doc)
        {
            var root = doc.RootElement;
            if (!root.TryGetProperty("type", out var typeProp))
            {
                return null;
            }

            var type = typeProp.GetString();
            if (type == "error")
            {
                return new AnthropicStreamCompletedEventArgs
                {
                    Reason = AnthropicStreamCompletionReason.Faulted,
                    ErrorMessage = ExtractApiErrorMessage(root)
                };
            }

            if (type == "content_block_delta"
                && root.TryGetProperty("delta", out var delta)
                && delta.TryGetProperty("type", out var deltaType)
                && deltaType.GetString() == "text_delta"
                && delta.TryGetProperty("text", out var textProp))
            {
                var text = textProp.GetString();
                if (!string.IsNullOrEmpty(text))
                {
                    ProgressChanged?.Invoke(this, new AnthropicStreamDeltaEventArgs(text));
                }
            }

            if (type == "message_stop")
            {
                return new AnthropicStreamCompletedEventArgs { Reason = AnthropicStreamCompletionReason.Completed };
            }
        }

        return null;
    }

    private static string ExtractApiErrorMessage(JsonElement root)
    {
        if (root.TryGetProperty("error", out var err) && err.ValueKind == JsonValueKind.Object)
        {
            if (err.TryGetProperty("message", out var msg))
            {
                return msg.GetString() ?? "Unknown API error";
            }

            if (err.TryGetProperty("type", out var errType))
            {
                return errType.GetString() ?? "Unknown API error";
            }
        }

        return "Unknown API error";
    }

    private void RaiseCompletedOnce(AnthropicStreamCompletedEventArgs args)
    {
        if (Interlocked.Exchange(ref _completionSent, 1) != 0)
        {
            return;
        }

        ProgressCompleted?.Invoke(this, args);
    }
}
