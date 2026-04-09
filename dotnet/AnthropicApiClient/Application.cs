using System.Text;
using FluentResults;
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
            await SendRequest("Write another sentence.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during application execution");
            throw;
        }
    }

    private async Task SendRequest(string message)
    {
        Result<string> result = await _anthropicClient.SendMessage(message);

        if (result.IsFailed)
        {
            _logger.LogError("API request failed: {Errors}", result.Errors);
            return;
        }

        _logger.LogInformation("API request successful!");
        _logger.LogInformation("Claude's response: {Response}", result.Value);

        StringBuilder contextDump = new();
        foreach (AnthropicMessage entry in _anthropicClient.Context)
        {
            contextDump.AppendLine(entry.ToString());
        }
        _logger.LogInformation("{Context}", contextDump.ToString());
    }
}