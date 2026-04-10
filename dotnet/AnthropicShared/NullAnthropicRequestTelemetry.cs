namespace AnthropicShared;

public sealed class NullAnthropicRequestTelemetry : IAnthropicRequestTelemetry
{
    public void LogOutgoingRequest(string httpMethod, string requestUri, string jsonBody)
    {
    }
}
