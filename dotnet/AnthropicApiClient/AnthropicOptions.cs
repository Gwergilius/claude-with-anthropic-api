namespace AnthropicApiClient;

public class AnthropicOptions
{
    public const string SectionName = "Anthropic";
    
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-sonnet-4-0";
    public string ApiVersion { get; set; } = "2023-06-01";

    public AnthropicOptions Validate()
    {
        if (string.IsNullOrEmpty(ApiKey))
        {
            throw new InvalidOperationException("Anthropic API key is required but not configured.");
        }
        
        if (string.IsNullOrEmpty(Model))
        {
            throw new InvalidOperationException("Anthropic Model is required but not configured.");
        }
        
        if (string.IsNullOrEmpty(ApiVersion))
        {
            throw new InvalidOperationException("Anthropic ApiVersion is required but not configured.");
        }

        return this;
    }
}