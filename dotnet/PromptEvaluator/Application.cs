using PromptEvaluator.Models;
using PromptEvaluator.Services;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PromptEvaluator;

public class Application(
    PromptEvaluatorService evaluatorService,
    IOptions<EvaluatorOptions> evaluatorOptions,
    IOptions<AnthropicOptions> anthropicOptions,
    IHostEnvironment hostEnvironment,
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

        logger.LogInformation(
            "Environment: {Environment} | Model: {Model}",
            hostEnvironment.EnvironmentName, anthropicOptions.Value.Model);

        var promptConfig = LoadYaml<PromptConfig>(opts.PromptFile);
        var dataset = LoadJson<List<TestCase>>(opts.DatasetFile);

        logger.LogInformation(
            "Starting evaluation: {Count} test cases from '{Dataset}', prompt: '{Prompt}'",
            dataset.Count, opts.DatasetFile, opts.PromptFile);

        var results = await evaluatorService.RunEvaluationAsync(
            dataset, promptConfig, opts.MaxConcurrentTasks);

        double avgScore = results.Count > 0 ? results.Average(r => r.Score) : 0;
        logger.LogInformation("Evaluation complete. Average score: {Score:F1}/10", avgScore);

        var jsonPath = Path.IsPathRooted(opts.JsonOutputFile) 
            ? opts.JsonOutputFile 
            : Path.Combine(AppContext.BaseDirectory, opts.JsonOutputFile);
        var json = JsonSerializer.Serialize(results, JsonWriteOptions);
        await File.WriteAllTextAsync(jsonPath, json);
        logger.LogInformation("Results written to {File}", jsonPath);

        var htmlPath = Path.IsPathRooted(opts.HtmlOutputFile)
            ? opts.HtmlOutputFile
            : Path.Combine(AppContext.BaseDirectory, opts.HtmlOutputFile);
        var html = ReportGenerator.GenerateHtml(results);
        await File.WriteAllTextAsync(htmlPath, html, System.Text.Encoding.UTF8);
        logger.LogInformation("HTML report written to {File}", htmlPath);
    }

    private T LoadYaml<T>(string path)
    {
        var fullPath = Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);
        
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Required file not found: {fullPath}", fullPath);

        return YamlDeserializer.Deserialize<T>(File.ReadAllText(fullPath))
            ?? throw new InvalidOperationException($"Failed to deserialize {fullPath}");
    }

    private T LoadJson<T>(string path)
    {
        var fullPath = Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);
        
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Required file not found: {fullPath}", fullPath);

        return JsonSerializer.Deserialize<T>(File.ReadAllText(fullPath), JsonReadOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize {fullPath}");
    }
}
