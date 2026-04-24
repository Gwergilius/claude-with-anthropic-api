
namespace AnthropicShared;

/// <summary>Observes outgoing Anthropic Messages API payloads (for diagnostics / UI).</summary>
public interface IAnthropicRequestTelemetry
{
    /// <param name="httpMethod">e.g. POST</param>
    /// <param name="requestUri">Full request URL</param>
    /// <param name="jsonBody">Serialized JSON body (no API key in body)</param>
    void LogOutgoingRequest(string httpMethod, string requestUri, string jsonBody);

    /// <summary>General-purpose diagnostic line for the dev log panel.</summary>
    void LogDiagnostic(LogLevel severity, string category, string message);
}
