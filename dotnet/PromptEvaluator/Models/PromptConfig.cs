
namespace PromptEvaluator.Models;

/// <summary>Represents the prompt under test, loaded from prompt.json.</summary>
public class PromptConfig
{
    /// <summary>What the prompt is supposed to accomplish — also passed to the grader as context.</summary>
    [JsonPropertyName("taskDescription")]
    public string TaskDescription { get; set; } = string.Empty;

    /// <summary>
    /// Prompt with <c>{key}</c> placeholders replaced by dataset prompt_inputs at runtime.
    /// Use <c>{{</c> / <c>}}</c> to include literal braces.
    /// </summary>
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Optional hard requirements passed to the grader as mandatory criteria.
    /// Any violation causes an automatic score of 3 or lower.
    /// </summary>
    [JsonPropertyName("extraCriteria")]
    public string? ExtraCriteria { get; set; }
}
