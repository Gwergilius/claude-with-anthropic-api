
namespace BlazorChat.Services;

public sealed record AppLogEntry(
    Guid Id,
    DateTimeOffset TimestampUtc,
    LogLevel Severity,
    string Category,
    string FullMessage);

/// <summary>In-memory ring buffer for the dev log panel (outgoing requests and diagnostics).</summary>
public sealed class AnthropicRequestLog : IAnthropicRequestTelemetry
{
    public const int MessagePreviewLength = 30;

    private readonly object _sync = new();
    private readonly List<AppLogEntry> _entries = [];
    private const int MaxEntries = 100;

    public event Action? Changed;

    public IReadOnlyList<AppLogEntry> GetSnapshot()
    {
        lock (_sync)
        {
            return [.. _entries];
        }
    }

    public void LogOutgoingRequest(string httpMethod, string requestUri, string jsonBody)
    {
        var formatted = TryFormatJson(jsonBody);
        var category = $"{httpMethod} {requestUri}";
        Append(LogLevel.Information, category, formatted);
    }

    public void LogDiagnostic(LogLevel severity, string category, string message)
    {
        Append(severity, category, message);
    }

    public static string FormatMessagePreview(string fullMessage, int maxLength = MessagePreviewLength)
    {
        if (string.IsNullOrEmpty(fullMessage))
        {
            return string.Empty;
        }

        var oneLine = fullMessage.ReplaceLineEndings(" ").Trim();
        while (oneLine.Contains("  ", StringComparison.Ordinal))
        {
            oneLine = oneLine.Replace("  ", " ", StringComparison.Ordinal);
        }

        return oneLine.Length <= maxLength ? oneLine : oneLine[..maxLength] + "…";
    }

    public static string FormatSeverityLabel(LogLevel level) => level switch
    {
        LogLevel.Trace => "Trace",
        LogLevel.Debug => "Debug",
        LogLevel.Information => "Info",
        LogLevel.Warning => "Warning",
        LogLevel.Error => "Error",
        LogLevel.Critical => "Critical",
        _ => level.ToString()
    };

    public static string SeverityCssClass(LogLevel level) =>
        "request-log-entry__severity--" + level.ToString().ToLowerInvariant();

    private void Append(LogLevel severity, string category, string fullMessage)
    {
        var entry = new AppLogEntry(Guid.NewGuid(), DateTimeOffset.UtcNow, severity, category, fullMessage);

        lock (_sync)
        {
            _entries.Add(entry);
            while (_entries.Count > MaxEntries)
            {
                _entries.RemoveAt(0);
            }
        }

        Changed?.Invoke();
    }

    private static string TryFormatJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (JsonException)
        {
            return json;
        }
    }
}
