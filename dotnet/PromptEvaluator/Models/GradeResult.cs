
namespace PromptEvaluator.Models;

/// <summary>Deserialized response from the grader LLM call.</summary>
internal class GradeResult
{
    [JsonPropertyName("strengths")]
    public List<string> Strengths { get; set; } = [];

    [JsonPropertyName("weaknesses")]
    public List<string> Weaknesses { get; set; } = [];

    [JsonPropertyName("reasoning")]
    public string Reasoning { get; set; } = string.Empty;

    [JsonPropertyName("score")]
    public int Score { get; set; }
}
