using AnthropicShared;
using BlazorChat.Services;
using FluentResults;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace BlazorChat.Pages;

public partial class Home : IDisposable
{
    [Inject] private IAntropicClient AnthropicClient { get; set; } = default!;
    [Inject] private IOptionsMonitor<AnthropicOptions> OptionsMonitor { get; set; } = default!;
    [Inject] private IAnthropicUserSettingsService UserSettings { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private IAnthropicStreamProgressService StreamProgress { get; set; } = default!;

    private IDisposable? _optionsChangeRegistration;

    /// <summary>Bumped after browser storage is merged so the config subtree remounts with correct field values.</summary>
    private int _configPanelKey;

    private bool _configPanelRemountedAfterStorage;

    private string errorMessage = string.Empty;
    private bool _isAwaitingHttp;
    private bool _isReadingStream;
    private CancellationTokenSource? _activeRequestCts;
    private TaskCompletionSource<AnthropicStreamCompletedEventArgs>? _streamDoneTcs;
    private AnthropicStreamingResponse? _pendingResponse;

    private bool IsChatBusy => _isAwaitingHttp || _isReadingStream;

    protected override void OnInitialized()
    {
        _optionsChangeRegistration = OptionsMonitor.OnChange(_ => InvokeAsync(StateHasChanged));
        StreamProgress.ProgressChanged += OnStreamProgressChanged;
        StreamProgress.ProgressCompleted += OnStreamProgressCompleted;
    }

    public void Dispose()
    {
        _optionsChangeRegistration?.Dispose();
        StreamProgress.ProgressChanged -= OnStreamProgressChanged;
        StreamProgress.ProgressCompleted -= OnStreamProgressCompleted;
        StreamProgress.Cancel();
        _activeRequestCts?.Cancel();
        _activeRequestCts?.Dispose();
        _pendingResponse?.Dispose();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await UserSettings.EnsureLoadedAsync();

        if (!UserSettings.IsBrowserStorageInitialized || _configPanelRemountedAfterStorage)
        {
            return;
        }

        _configPanelRemountedAfterStorage = true;
        _configPanelKey++;
        await InvokeAsync(StateHasChanged);
    }

    private void OnStreamProgressChanged(object? sender, AnthropicStreamDeltaEventArgs e)
    {
        _ = InvokeAsync(() =>
        {
            AnthropicClient.AppendLastAssistantMessageText(e.DeltaText);
            StateHasChanged();
        });
    }

    private void OnStreamProgressCompleted(object? sender, AnthropicStreamCompletedEventArgs e)
    {
        _streamDoneTcs?.TrySetResult(e);

        _ = InvokeAsync(() =>
        {
            _isReadingStream = false;
            if (e.Reason == AnthropicStreamCompletionReason.Faulted)
            {
                errorMessage = e.ErrorMessage ?? "The response stream failed.";
            }

            StateHasChanged();
        });
    }

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

        StreamProgress.Cancel();
        _activeRequestCts?.Cancel();
        _activeRequestCts?.Dispose();
        _pendingResponse?.Dispose();
        _pendingResponse = null;

        _activeRequestCts = new CancellationTokenSource();
        var ct = _activeRequestCts.Token;

        _isAwaitingHttp = true;
        StateHasChanged();

        try
        {
            Result<AnthropicStreamingResponse> result = await AnthropicClient.StartStreamingMessageAsync(
                input,
                OptionsMonitor.CurrentValue.SystemPrompt,
                ct);

            if (result.IsFailed)
            {
                errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                return;
            }

            _pendingResponse = result.Value;
            _isAwaitingHttp = false;
            _isReadingStream = true;
            StateHasChanged();

            _streamDoneTcs = new TaskCompletionSource<AnthropicStreamCompletedEventArgs>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            StreamProgress.Start(_pendingResponse.Body, ct);
            await _streamDoneTcs.Task;
        }
        catch (OperationCanceledException)
        {
            // Stop button or navigation — optional user-facing message suppressed
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
        finally
        {
            _isAwaitingHttp = false;
            _isReadingStream = false;
            _pendingResponse?.Dispose();
            _pendingResponse = null;
            _activeRequestCts?.Dispose();
            _activeRequestCts = null;
            await InvokeAsync(StateHasChanged);
        }
    }

    private void HandleStopStreaming()
    {
        _activeRequestCts?.Cancel();
        StreamProgress.Cancel();
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
