using System.Text.Json;
using AnthropicShared;
using Microsoft.Extensions.Options;

namespace BlazorChat.Services;

/// <summary>
/// Live <see cref="AnthropicOptions"/> snapshot merged from appsettings and browser overrides.
/// </summary>
public sealed class RuntimeAnthropicOptions : IOptions<AnthropicOptions>, IOptionsMonitor<AnthropicOptions>
{
    private readonly object _sync = new();
    private AnthropicOptions _current;
    private readonly List<Action<AnthropicOptions, string?>> _listeners = [];

    public RuntimeAnthropicOptions(AnthropicOptions initialFromConfiguration)
    {
        ArgumentNullException.ThrowIfNull(initialFromConfiguration);
        _current = Clone(initialFromConfiguration);
    }

    public AnthropicOptions Value
    {
        get
        {
            lock (_sync)
            {
                return _current;
            }
        }
    }

    public AnthropicOptions CurrentValue => Value;

    public AnthropicOptions Get(string? name) => Value;

    public IDisposable OnChange(Action<AnthropicOptions, string?> listener)
    {
        ArgumentNullException.ThrowIfNull(listener);
        lock (_sync)
        {
            _listeners.Add(listener);
        }

        return new Unregister(this, listener);
    }

    /// <summary>Apply values from the configuration panel (all fields are intentional, including empty system prompt).</summary>
    public void ApplyFormState(AnthropicUserSettingsDto user)
    {
        ArgumentNullException.ThrowIfNull(user);

        lock (_sync)
        {
            if (!string.IsNullOrWhiteSpace(user.Model))
            {
                _current.Model = user.Model.Trim();
            }

            _current.Temperature = Math.Clamp(user.Temperature, 0d, 1d);
            _current.SystemPrompt = user.SystemPrompt ?? string.Empty;
        }

        NotifyChanged();
    }

    /// <summary>Merge fields present in browser storage; <c>null</c> means “leave current (e.g. appsettings) value”.</summary>
    public void MergePersistedOverrides(string? model, double? temperature, string? systemPrompt)
    {
        lock (_sync)
        {
            if (!string.IsNullOrWhiteSpace(model))
            {
                _current.Model = model.Trim();
            }

            if (temperature is double t)
            {
                _current.Temperature = Math.Clamp(t, 0d, 1d);
            }

            if (systemPrompt is not null)
            {
                _current.SystemPrompt = systemPrompt;
            }
        }

        NotifyChanged();
    }

    private void NotifyChanged()
    {
        AnthropicOptions snapshot;
        lock (_sync)
        {
            snapshot = _current;
        }

        List<Action<AnthropicOptions, string?>> copy;
        lock (_sync)
        {
            copy = [.. _listeners];
        }

        foreach (var listener in copy)
        {
            listener(snapshot, Options.DefaultName);
        }
    }

    private void RemoveListener(Action<AnthropicOptions, string?> listener)
    {
        lock (_sync)
        {
            _listeners.Remove(listener);
        }
    }

    private sealed class Unregister(RuntimeAnthropicOptions owner, Action<AnthropicOptions, string?> listener) : IDisposable
    {
        public void Dispose() => owner.RemoveListener(listener);
    }

    private static AnthropicOptions Clone(AnthropicOptions source)
    {
        // Shallow copy of POCO — sufficient for AnthropicOptions (no nested mutable refs).
        return JsonSerializer.Deserialize<AnthropicOptions>(JsonSerializer.Serialize(source))
            ?? throw new InvalidOperationException("Failed to clone AnthropicOptions.");
    }
}
