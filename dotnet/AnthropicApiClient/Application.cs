
namespace AnthropicApiClient;

public class Application(IAntropicClient anthropicClient, ILogger<Application> logger)
{
    private readonly IAntropicClient _anthropicClient = anthropicClient ?? throw new ArgumentNullException(nameof(anthropicClient));
    private readonly ILogger<Application> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task Run()
    {
        Console.WriteLine("Claude chat — type 'exit' to quit.");
        Console.WriteLine();
        Console.WriteLine("Do you want to specify any system prompt? ");
        var systemPrompt = Console.ReadLine();
        if (systemPrompt == null || systemPrompt.Length == 0 || (bool.TryParse(systemPrompt, out var answer) && !answer))
        {
            systemPrompt = null;
        }

        while (true)
        {
            Console.Write("You: ");
            string? input = Console.ReadLine();

            if (input is null || input.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            if (string.IsNullOrWhiteSpace(input))
                continue;

            Result<string> result = await _anthropicClient.SendMessage(input, systemPrompt);

            if (result.IsFailed)
            {
                _logger.LogError("API request failed: {Errors}", result.Errors);
                Console.WriteLine("Error: could not get a response.");
                continue;
            }

            Console.WriteLine($"Claude: {result.Value}");
            Console.WriteLine();
        }
    }
}