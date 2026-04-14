using AnthropicShared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorChat.Components;

public partial class ChatHistory
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    [Parameter] public IEnumerable<AnthropicMessage> Messages { get; set; } = [];
    [Parameter] public bool IsAwaitingHttp { get; set; }
    [Parameter] public bool StreamPending { get; set; }
    [Parameter] public EventCallback OnStopStreaming { get; set; }

    private List<AnthropicMessage> _messages = [];
    private ElementReference historyRef;

    protected override void OnParametersSet()
    {
        _messages = Messages.ToList();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await JS.InvokeVoidAsync("scrollToBottom", historyRef);
    }
}
