
namespace AnthropicShared;

public class AnthropicOptions
{
    public const string SectionName = "Anthropic";

    [Required] public string ApiKey { get; set; } = string.Empty;
    [Required] public string Model { get; set; } = "claude-sonnet-4-5";
    [Required] public string ApiVersion { get; set; } = "2023-06-01";
    public int MaxTokens { get; set; } = 1000;
    public double Temperature { get; set; } = 0;

    /// <summary>Default system prompt for chat requests when no per-call override is provided.</summary>
    public string SystemPrompt { get; set; } = string.Empty;
}
