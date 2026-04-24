
namespace PromptEvaluator.Models;

/// <summary>Result for one test case: the model output, grading score, and reasoning.</summary>
public class EvaluationResult
{
    [JsonPropertyName("output")]
    public string Output { get; set; } = string.Empty;

    [JsonPropertyName("test_case")]
    public TestCase TestCase { get; set; } = new();

    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("reasoning")]
    public string Reasoning { get; set; } = string.Empty;
}
