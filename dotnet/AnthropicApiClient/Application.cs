using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AnthropicApiClient;

public class Application(IAntropicClient anthropicClient, ILogger<Application> logger)
{
    private readonly IAntropicClient _anthropicClient = anthropicClient ?? throw new ArgumentNullException(nameof(anthropicClient));
    private readonly ILogger<Application> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task Run()
    {
        try
        {
            _logger.LogInformation("AnthropicClient resolved from DI container");
            
            // Send first request
            await SendRequest("What is quantum computing? Answer in one sentence.");
            
            _logger.LogInformation("Making second API call");
            
            // Send second request
            await SendRequest("What is dependency injection? Answer in one sentence.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during application execution");
            throw;
        }
    }

    private async Task SendRequest(string message)
    {
        // Send message using the client
        using JsonDocument response = await _anthropicClient.SendMessage(message);
        
        _logger.LogInformation("API request successful!");
        
        // Parse the response JSON
        JsonElement root = response.RootElement;
        
        if (root.TryGetProperty("content", out JsonElement contentArray) && contentArray.ValueKind == JsonValueKind.Array)
        {
            JsonElement firstContent = contentArray[0];
            if (firstContent.TryGetProperty("text", out JsonElement textElement))
            {
                string claudeResponse = textElement.GetString() ?? "";
                _logger.LogInformation("Claude's response: {Response}", claudeResponse);
            }
        }
    }
}