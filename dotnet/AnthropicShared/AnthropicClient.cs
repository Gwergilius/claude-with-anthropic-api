
namespace AnthropicShared;

public class AnthropicClient(
    IHttpClientFactory httpClientFactory,
    IOptionsMonitor<AnthropicOptions> optionsMonitor,
    ILogger<AnthropicClient> logger,
    IAnthropicRequestTelemetry requestTelemetry) : IAntropicClient, IDisposable
{
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";

    private static readonly JsonSerializerOptions RequestJsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly List<AnthropicMessage> _context = [];
    private bool _disposed = false;

    public IEnumerable<AnthropicMessage> Context => _context;

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

    public async Task<Result<string>> SendMessage(string message, string? systemPrompt = null, double? temperatureOverride = null)
    {
        var config = optionsMonitor.CurrentValue;

        _context.Add(new UserMessage(message));

        var explicitSystem = string.IsNullOrWhiteSpace(systemPrompt) ? null : systemPrompt.Trim();
        var fromOptions = string.IsNullOrWhiteSpace(config.SystemPrompt) ? null : config.SystemPrompt.Trim();
        var effectiveSystem = explicitSystem ?? fromOptions;

        var body = new AnthropicMessagesApiRequest
        {
            Model = config.Model,
            MaxTokens = config.MaxTokens,
            Temperature = temperatureOverride ?? config.Temperature,
            Messages = _context,
            System = effectiveSystem
        };

        string requestJson = JsonSerializer.Serialize(body, RequestJsonOptions);
        requestTelemetry.LogOutgoingRequest("POST", ApiUrl, requestJson);

        // Create HTTP content
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Make the API request
        var httpClient = CreateClient(config);
        HttpResponseMessage response = await httpClient.PostAsync(ApiUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            logger.LogWarning("Anthropic API error {Status}: {Body}", response.StatusCode, errorContent);
            requestTelemetry.LogDiagnostic(
                LogLevel.Warning,
                $"POST {ApiUrl} → {(int)response.StatusCode} {response.StatusCode}",
                errorContent);
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

    public async Task<Result<AnthropicStreamingResponse>> StartStreamingMessageAsync(
        string message,
        string? systemPrompt = null,
        CancellationToken cancellationToken = default)
    {
        var config = optionsMonitor.CurrentValue;

        _context.Add(new UserMessage(message));

        var explicitSystem = string.IsNullOrWhiteSpace(systemPrompt) ? null : systemPrompt.Trim();
        var fromOptions = string.IsNullOrWhiteSpace(config.SystemPrompt) ? null : config.SystemPrompt.Trim();
        var effectiveSystem = explicitSystem ?? fromOptions;

        var body = new AnthropicMessagesApiRequest
        {
            Model = config.Model,
            MaxTokens = config.MaxTokens,
            Temperature = config.Temperature,
            Messages = _context,
            System = effectiveSystem,
            Stream = true
        };

        string requestJson = JsonSerializer.Serialize(body, RequestJsonOptions);
        requestTelemetry.LogOutgoingRequest("POST", ApiUrl, requestJson);

        using var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
        using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl) { Content = content };

        var httpClient = CreateClient(config);
        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Result.Fail("Request cancelled.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Anthropic streaming request failed before response headers");
            return Result.Fail($"Request failed: {ex.Message}");
        }

        if (!response.IsSuccessStatusCode)
        {
            try
            {
                string errorContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                var status = response.StatusCode;
                logger.LogWarning("Anthropic API error {Status}: {Body}", status, errorContent);
                requestTelemetry.LogDiagnostic(
                    LogLevel.Warning,
                    $"POST {ApiUrl} → {(int)status} {status}",
                    errorContent);
                return Result.Fail($"API request failed with status {status}: {errorContent}");
            }
            finally
            {
                response.Dispose();
            }
        }

        Stream stream;
        try
        {
            stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            response.Dispose();
            logger.LogError(ex, "Failed to open Anthropic response stream");
            return Result.Fail($"Failed to open response stream: {ex.Message}");
        }

        _context.Add(new AssistantMessage(""));
        return Result.Ok(new AnthropicStreamingResponse(response, stream));
    }

    public void AppendLastAssistantMessageText(string delta)
    {
        if (string.IsNullOrEmpty(delta) || _context.Count == 0)
        {
            return;
        }

        if (_context[^1] is AssistantMessage am)
        {
            _context[^1] = new AssistantMessage(am.Content + delta);
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
