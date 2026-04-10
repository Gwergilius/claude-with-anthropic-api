using System.Text.Json;
using AnthropicShared;
using Microsoft.JSInterop;

namespace BlazorChat.Services;

public sealed class AnthropicUserSettingsService(
    IJSRuntime jsRuntime,
    RuntimeAnthropicOptions runtimeOptions) : IAnthropicUserSettingsService
{
    private const string StorageKey = "blazorChat.anthropic.userSettings";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = false };

    /// <summary>Shape read from localStorage (nullable = property omitted in JSON).</summary>
    private sealed class AnthropicUserSettingsFileDto
    {
        public string? Model { get; set; }
        public double? Temperature { get; set; }
        public string? SystemPrompt { get; set; }
    }

    private readonly SemaphoreSlim _loadGate = new(1, 1);
    private bool _loaded;

    public bool IsBrowserStorageInitialized => _loaded;

    public void ApplyLive(AnthropicUserSettingsDto settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        runtimeOptions.ApplyFormState(settings);
    }

    public AnthropicUserSettingsDto GetEditingSnapshot()
    {
        var v = runtimeOptions.CurrentValue;
        return new AnthropicUserSettingsDto
        {
            Model = v.Model,
            Temperature = Math.Clamp(v.Temperature, 0d, 1d),
            SystemPrompt = v.SystemPrompt ?? string.Empty
        };
    }

    public async Task EnsureLoadedAsync(CancellationToken cancellationToken = default)
    {
        if (_loaded)
        {
            return;
        }

        await _loadGate.WaitAsync(cancellationToken);
        try
        {
            if (_loaded)
            {
                return;
            }

            string? json = null;
            try
            {
                json = await jsRuntime.InvokeAsync<string?>("blazorChat.storage.getItem", cancellationToken, StorageKey);
            }
            catch (InvalidOperationException)
            {
                // Static prerender or circuit not attached yet — retry on a later render.
                return;
            }
            catch (JSException)
            {
                // No JS — keep appsettings-only options.
            }

            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    var stored = JsonSerializer.Deserialize<AnthropicUserSettingsFileDto>(json, JsonOptions);
                    if (stored != null)
                    {
                        runtimeOptions.MergePersistedOverrides(stored.Model, stored.Temperature, stored.SystemPrompt);
                    }
                }
                catch (JsonException)
                {
                    // Ignore corrupt payload.
                }
            }

            _loaded = true;
        }
        finally
        {
            _loadGate.Release();
        }
    }

    public async Task SaveAsync(AnthropicUserSettingsDto settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var normalized = new AnthropicUserSettingsDto
        {
            Model = settings.Model?.Trim() ?? string.Empty,
            Temperature = Math.Clamp(settings.Temperature, 0d, 1d),
            SystemPrompt = settings.SystemPrompt ?? string.Empty
        };

        runtimeOptions.ApplyFormState(normalized);

        var json = JsonSerializer.Serialize(normalized, JsonOptions);
        try
        {
            await jsRuntime.InvokeVoidAsync("blazorChat.storage.setItem", cancellationToken, StorageKey, json);
        }
        catch (JSException)
        {
            // Options still updated in memory for this session.
        }
    }
}
