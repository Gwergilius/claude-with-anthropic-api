using System.Text;
using System.Text.Json;
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
    private bool _disposed = false;

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

    public async Task<JsonDocument> SendMessage(string message)
    {
        // Prepare the request data
        var requestData = new
        {
            model = _config.Model,
            max_tokens = 1000,
            temperature = 0,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = message
                }
            }
        };

        // Serialize request data to JSON
        string requestJson = JsonSerializer.Serialize(requestData);

        // Create HTTP content
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Make the API request
        var httpClient = CreateClient();
        HttpResponseMessage response = await httpClient.PostAsync(ApiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            string responseJson = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(responseJson);
        }
        else
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"API request failed with status {response.StatusCode}: {errorContent}");
        }
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