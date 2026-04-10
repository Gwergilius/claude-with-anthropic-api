using System.Text.Json;
using AnthropicShared;

namespace BlazorChat.Services;

public sealed record AnthropicRequestLogEntry(DateTimeOffset TimestampUtc, string Headline, string FormattedBody);

/// <summary>In-memory ring buffer of outgoing Anthropic API request bodies for the dev UI.</summary>
public sealed class AnthropicRequestLog : IAnthropicRequestTelemetry
{
    private readonly object _sync = new();
    private readonly List<AnthropicRequestLogEntry> _entries = [];
    private const int MaxEntries = 100;

    public event Action? Changed;

    public IReadOnlyList<AnthropicRequestLogEntry> GetSnapshot()
    {
        lock (_sync)
        {
            return [.. _entries];
        }
    }

    public void LogOutgoingRequest(string httpMethod, string requestUri, string jsonBody)
    {
        var formatted = TryFormatJson(jsonBody);
        var entry = new AnthropicRequestLogEntry(DateTimeOffset.UtcNow, $"{httpMethod} {requestUri}", formatted);

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
