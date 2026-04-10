namespace BlazorChat.Services;

public static class AnthropicModelCatalog
{
    public static IReadOnlyList<string> Models { get; } =
    [
        "claude-3-5-haiku-20241022",
        "claude-3-5-sonnet-20241022",
        "claude-3-opus-20240229",
        "claude-haiku-4-5",
        "claude-haiku-4-6",
        "claude-opus-4-5",
        "claude-opus-4-6",
        "claude-sonnet-4-5",
        "claude-sonnet-4-6"
    ];
}
