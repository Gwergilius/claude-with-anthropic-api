
namespace AnthropicShared;

public record AnthropicMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content)
{
    public override string ToString() => $"[{Role}]: {Content}";
}

public record UserMessage(string Content)
    : AnthropicMessage("user", Content);

public record AssistantMessage(string Content)
    : AnthropicMessage("assistant", Content);
