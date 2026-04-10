namespace BlazorChat.Services;

/// <summary>Loads and saves chat-related Anthropic overrides from browser storage.</summary>
public interface IAnthropicUserSettingsService
{
    /// <summary><c>true</c> after a successful browser read (or confirmed empty) once the JS runtime allows interop.</summary>
    bool IsBrowserStorageInitialized { get; }

    /// <summary>Ensures storage has been read once for this circuit and merged into runtime options.</summary>
    Task EnsureLoadedAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists settings and updates <see cref="AnthropicOptions"/> for the current session.</summary>
    Task SaveAsync(AnthropicUserSettingsDto settings, CancellationToken cancellationToken = default);

    /// <summary>Updates effective <see cref="AnthropicOptions"/> immediately (no localStorage write).</summary>
    void ApplyLive(AnthropicUserSettingsDto settings);

    /// <summary>Current user-facing values (after load / last save).</summary>
    AnthropicUserSettingsDto GetEditingSnapshot();
}
