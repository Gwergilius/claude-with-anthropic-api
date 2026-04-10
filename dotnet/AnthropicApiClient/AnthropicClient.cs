using System.Text;
using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AnthropicApiClient;

public class AnthropicClient(
    IHttpClientFactory httpClientFactory, 
    IOptions<AnthropicOptions> options, 
    ILogger<AnthropicClient> logger) : IAntropicClient, IDisposable
{
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";
    private readonly AnthropicOptions _config = InitializeOptions(options, logger);
    private readonly List<AnthropicMessage> _context = [];
    private bool _disposed = false;

    public IEnumerable<AnthropicMessage> Context => _context;

    private static AnthropicOptions InitializeOptions(IOptions<AnthropicOptions> options, ILogger<AnthropicClient> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        
        // Validate configuration before using it
        var validatedOptions = options.Value.Validate();
        
        logger.LogInformation("API key loaded successfully");
        logger.LogInformation("Using model: {Model}", validatedOptions.Model);
        
        return validatedOptions;
    }

    private HttpClient CreateClient() => CreateClient(_config);
    private HttpClient CreateClient(AnthropicOptions config)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        
        var httpClient = httpClientFactory.CreateClient();

        // Set up headers
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("anthropic-version", config.ApiVersion);
        httpClient.DefaultRequestHeaders.Add("x-api-key", config.ApiKey);

        return httpClient;
    }

    public async Task<Result<string>> SendMessage(string message, string? systemPrompt = null)
    {
        _context.Add(new UserMessage(message));

        // Prepare the request data
        Dictionary<string, object> requestData = new()
        {
            ["model"] = _config.Model,
            ["max_tokens"] = _config.MaxTokens,
            ["temperature"] = _config.Temperature,
            ["messages"] = _context
        };
        if(systemPrompt is { Length: > 0 })
        {
            requestData["system"] = systemPrompt;
        }

        // Serialize request data to JSON
        string requestJson = JsonSerializer.Serialize(requestData);

        // Create HTTP content
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Make the API request
        var httpClient = CreateClient();
        HttpResponseMessage response = await httpClient.PostAsync(ApiUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            return Result.Fail($"API request failed with status {response.StatusCode}: {errorContent}");
        }

        string responseJson = await response.Content.ReadAsStringAsync();
        using JsonDocument document = JsonDocument.Parse(responseJson);

        string assistantMessage = document.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;

        _context.Add(new AssistantMessage(assistantMessage));

        return Result.Ok(assistantMessage);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _disposed = true;
        }
    }
}