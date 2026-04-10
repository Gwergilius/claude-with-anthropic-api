namespace BlazorChat.Services;

/// <summary>User-editable fields persisted in the browser (localStorage).</summary>
public sealed class AnthropicUserSettingsDto
{
    public string Model { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public string SystemPrompt { get; set; } = string.Empty;
}
