using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazorChat.Components;

public partial class ChatInput
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    /// <summary>Disables input and button while the AI response is in progress.</summary>
    [Parameter] public bool IsDisabled { get; set; }

    /// <summary>Raised with the trimmed message text when the user sends a message.</summary>
    [Parameter] public EventCallback<string> OnSend { get; set; }

    private string userInput = string.Empty;
    private ElementReference inputRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("focusElement", inputRef);
        }
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey && !IsDisabled)
        {
            await HandleSend();
        }
    }

    private async Task HandleSend()
    {
        var input = userInput.Trim();
        if (string.IsNullOrEmpty(input) || IsDisabled)
            return;

        userInput = string.Empty;

        // Await the parent handler (API call); focus is restored afterwards.
        await OnSend.InvokeAsync(input);

        await JS.InvokeVoidAsync("focusElement", inputRef);
    }
}
