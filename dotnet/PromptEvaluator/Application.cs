using PromptEvaluator.Models;
using PromptEvaluator.Services;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PromptEvaluator;

public class Application(
    PromptEvaluatorService evaluatorService,
    IOptions<EvaluatorOptions> evaluatorOptions,
    ILogger<Application> logger)
{
    private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    private static readonly JsonSerializerOptions JsonReadOptions = new() { PropertyNameCaseInsensitive = true };
    private static readonly JsonSerializerOptions JsonWriteOptions = new() { WriteIndented = true };

    public async Task Run()
    {
        var opts = evaluatorOptions.Value;

        var promptConfig = LoadYaml<PromptConfig>(opts.PromptFile);
        var dataset = LoadJson<List<TestCase>>(opts.DatasetFile);

        logger.LogInformation(
            "Starting evaluation: {Count} test cases from '{Dataset}', prompt: '{Prompt}'",
            dataset.Count, opts.DatasetFile, opts.PromptFile);

        var results = await evaluatorService.RunEvaluationAsync(
            dataset, promptConfig, opts.MaxConcurrentTasks);

        double avgScore = results.Count > 0 ? results.Average(r => r.Score) : 0;
        logger.LogInformation("Evaluation complete. Average score: {Score:F1}/10", avgScore);

        var json = JsonSerializer.Serialize(results, JsonWriteOptions);
        await File.WriteAllTextAsync(opts.JsonOutputFile, json);
        logger.LogInformation("Results written to {File}", opts.JsonOutputFile);

        var html = ReportGenerator.GenerateHtml(results);
        await File.WriteAllTextAsync(opts.HtmlOutputFile, html, System.Text.Encoding.UTF8);
        logger.LogInformation("HTML report written to {File}", opts.HtmlOutputFile);
    }

    private T LoadYaml<T>(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Required file not found: {path}", path);

        return YamlDeserializer.Deserialize<T>(File.ReadAllText(path))
            ?? throw new InvalidOperationException($"Failed to deserialize {path}");
    }

    private T LoadJson<T>(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Required file not found: {path}", path);

        return JsonSerializer.Deserialize<T>(File.ReadAllText(path), JsonReadOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize {path}");
    }
}
