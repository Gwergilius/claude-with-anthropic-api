namespace AnthropicShared;

/// <summary>Owns the HTTP response and the response body stream until disposed.</summary>
public sealed class AnthropicStreamingResponse : IDisposable
{
    private readonly HttpResponseMessage _response;

    internal AnthropicStreamingResponse(HttpResponseMessage response, Stream body)
    {
        _response = response;
        Body = body;
    }

    public Stream Body { get; }

    public void Dispose()
    {
        Body.Dispose();
        _response.Dispose();
    }
}
