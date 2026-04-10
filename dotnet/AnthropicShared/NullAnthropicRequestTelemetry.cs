using Microsoft.Extensions.Logging;

namespace AnthropicShared;

public sealed class NullAnthropicRequestTelemetry : IAnthropicRequestTelemetry
{
    public void LogOutgoingRequest(string httpMethod, string requestUri, string jsonBody)
    {
    }

    public void LogDiagnostic(LogLevel severity, string category, string message)
    {
    }
}
