using PromptEvaluator.Models;

namespace PromptEvaluator.Services;

public class PromptEvaluatorService(
    IServiceScopeFactory scopeFactory,
    ILogger<PromptEvaluatorService> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<List<EvaluationResult>> RunEvaluationAsync(
        List<TestCase> dataset,
        PromptConfig promptConfig,
        int maxConcurrentTasks = 3,
        CancellationToken cancellationToken = default)
    {
        var results = new ConcurrentBag<EvaluationResult>();
        var semaphore = new SemaphoreSlim(maxConcurrentTasks);
        int completed = 0;
        int total = dataset.Count;

        var tasks = dataset.Select(async testCase =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var result = await RunTestCaseAsync(testCase, promptConfig, cancellationToken);
                results.Add(result);
                int count = Interlocked.Increment(ref completed);
                logger.LogInformation("Evaluated {Completed}/{Total} — score: {Score}", count, total, result.Score);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        return [.. results];
    }

    private async Task<EvaluationResult> RunTestCaseAsync(
        TestCase testCase,
        PromptConfig promptConfig,
        CancellationToken cancellationToken)
    {
        var output = await RunPromptAsync(testCase.PromptInputs, promptConfig.Prompt, cancellationToken);
        var grade = await GradeOutputAsync(testCase, output, promptConfig.ExtraCriteria, cancellationToken);

        return new EvaluationResult
        {
            Output = output,
            TestCase = testCase,
            Score = grade.Score,
            Reasoning = grade.Reasoning,
        };
    }

    private async Task<string> RunPromptAsync(
        Dictionary<string, string> promptInputs,
        string template,
        CancellationToken cancellationToken)
    {
        var rendered = Render(template, promptInputs);
        return await CallClaudeAsync(rendered, systemPrompt: null, temperature: null, cancellationToken);
    }

    private async Task<GradeResult> GradeOutputAsync(
        TestCase testCase,
        string output,
        string? extraCriteria,
        CancellationToken cancellationToken)
    {
        var gradingPrompt = BuildGradingPrompt(testCase, output, extraCriteria);
        const string system = "You are an expert evaluator. Respond with only a JSON object, no other text.";
        var response = await CallClaudeAsync(gradingPrompt, system, temperature: 0.0, cancellationToken);
        return ParseGradeResult(response);
    }

    private async Task<string> CallClaudeAsync(
        string message,
        string? systemPrompt,
        double? temperature,
        CancellationToken cancellationToken)
    {
        // Use a fresh scope so each call gets a stateless AnthropicClient (no context carryover).
        using var scope = scopeFactory.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<IAntropicClient>();
        var result = await client.SendMessage(message, systemPrompt, temperature);
        if (result.IsFailed)
            
            throw new InvalidOperationException(
                $"API call failed: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        return result.Value;
    }

    private static string BuildGradingPrompt(TestCase testCase, string output, string? extraCriteria)
    {
        var inputsText = string.Join(",\n", testCase.PromptInputs.Select(kv =>
            $"  \"{kv.Key}\": \"{kv.Value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n")}\""));

        var extraSection = string.IsNullOrWhiteSpace(extraCriteria) ? string.Empty : $$"""

        Mandatory Requirements - ANY VIOLATION MEANS AUTOMATIC FAILURE (score of 3 or lower):
        <extra_important_criteria>
        {{extraCriteria}}
        </extra_important_criteria>
        """;

        return $$"""
        Your task is to evaluate the following AI-generated solution with EXTREME RIGOR.

        Original task description:
        <task_description>
        {{testCase.TaskDescription}}
        </task_description>

        Original task inputs:
        <task_inputs>
        {
        {{inputsText}}
        }
        </task_inputs>

        Solution to Evaluate:
        <solution>
        {{output}}
        </solution>

        Criteria you should use to evaluate the solution:
        <criteria>
        {{string.Join("\n", testCase.SolutionCriteria)}}
        </criteria>
        {{extraSection}}
        Scoring Guidelines:
        * Score 1-3: Solution fails to meet one or more MANDATORY requirements
        * Score 4-6: Solution meets all mandatory requirements but has significant deficiencies in secondary criteria
        * Score 7-8: Solution meets all mandatory requirements and most secondary criteria, with minor issues
        * Score 9-10: Solution meets all mandatory and secondary criteria

        IMPORTANT SCORING INSTRUCTIONS:
        * Grade the output based ONLY on the listed criteria. Do not add your own extra requirements.
        * If a solution meets all of the mandatory and secondary criteria give it a 10
        * ANY violation of a mandatory requirement MUST result in a score of 3 or lower
        * The full 1-10 scale should be utilized — don't hesitate to give low scores when warranted

        Respond with only a JSON object in this exact format:
        {
            "strengths": ["strength1", "strength2"],
            "weaknesses": ["weakness1"],
            "reasoning": "concise explanation",
            "score": 8
        }
        """;
    }

    private GradeResult ParseGradeResult(string response)
    {
        var match = Regex.Match(response, @"\{[\s\S]*\}", RegexOptions.Multiline);
        if (!match.Success)
        {
            logger.LogWarning("Could not extract JSON from grading response: {Response}", response);
            return new GradeResult { Score = 5, Reasoning = "Failed to parse grading response" };
        }

        try
        {
            return JsonSerializer.Deserialize<GradeResult>(match.Value, JsonOptions)
                ?? new GradeResult { Score = 5, Reasoning = "Empty grading response" };
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to deserialize grading response");
            return new GradeResult { Score = 5, Reasoning = "Failed to deserialize grading response" };
        }
    }

    /// <summary>
    /// Replaces <c>{key}</c> placeholders in <paramref name="template"/> with values from
    /// <paramref name="variables"/>. Escaped double-braces <c>{{</c>/<c>}}</c> become literal braces.
    /// </summary>
    private static string Render(string template, Dictionary<string, string> variables)
    {
        var result = new StringBuilder(template);
        foreach (var (key, value) in variables)
            result.Replace($"{{{key}}}", value);
        return result.Replace("{{", "{").Replace("}}", "}").ToString();
    }
}
