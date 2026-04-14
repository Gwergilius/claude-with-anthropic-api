using System.Text.Json.Serialization;

namespace AnthropicShared;

/// <summary>POST /v1/messages body — strongly typed so System.Text.Json always emits every field correctly.</summary>
internal sealed class AnthropicMessagesApiRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("max_tokens")]
    public required int MaxTokens { get; init; }

    [JsonPropertyName("temperature")]
    public required double Temperature { get; init; }

    [JsonPropertyName("messages")]
    public required List<AnthropicMessage> Messages { get; init; }

    /// <summary>Omitted from JSON when null (Anthropic accepts plain string for simple system instructions).</summary>
    [JsonPropertyName("system")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? System { get; init; }

    /// <summary>When true, the API returns an SSE stream instead of a single JSON body.</summary>
    [JsonPropertyName("stream")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Stream { get; init; }
}
