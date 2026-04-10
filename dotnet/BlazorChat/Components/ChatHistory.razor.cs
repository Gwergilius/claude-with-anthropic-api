using AnthropicShared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorChat.Components;

public partial class ChatHistory
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    [Parameter] public IEnumerable<AnthropicMessage> Messages { get; set; } = [];
    [Parameter] public bool IsLoading { get; set; }

    private ElementReference historyRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await JS.InvokeVoidAsync("scrollToBottom", historyRef);
    }
}
