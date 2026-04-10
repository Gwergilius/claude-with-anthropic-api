using AnthropicShared;
using FluentResults;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace BlazorChat.Pages;

public partial class Home
{
    [Inject] private IAntropicClient AnthropicClient { get; set; } = default!;
    [Inject] private IOptions<AnthropicOptions> Options { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private string errorMessage = string.Empty;
    private bool isLoading = false;

    private async Task HandleSend(string input)
    {
        // Slash commands are handled locally — not forwarded to the AI
        if (input.StartsWith('/'))
        {
            await HandleCommand(input);
        }
        else
        {
            await HandleMessage(input);
        }

    }

    private async Task HandleMessage(string input)
    {
        errorMessage = string.Empty;

        isLoading = true;

        try
        {
            Result<string> result = await AnthropicClient.SendMessage(input);

            if (result.IsFailed)
            {
                errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
            }
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task HandleCommand(string rawInput)
    {
        var command = rawInput.Split(' ')[0].ToLowerInvariant();

        switch (command)
        {
            case "/exit":
                await JS.InvokeVoidAsync("window.close");
                break;
            case "/error":
                var message = rawInput.Length > "/error".Length
                    ? rawInput["/error".Length..].Trim()
                    : "This is a test error message.";
                errorMessage = message;
                break;
            default:
                errorMessage = $"Unknown command: {rawInput}";
                break;
        }
    }
}
