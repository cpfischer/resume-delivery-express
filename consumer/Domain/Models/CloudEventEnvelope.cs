using System.Text.Json.Serialization;

namespace Consumer.Api.Domain.Services.Models;

public sealed class CloudEventEnvelope
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("specversion")]
    public string SpecVersion { get; set; } = string.Empty;

    [JsonPropertyName("time")]
    public DateTimeOffset Time { get; set; }

    [JsonPropertyName("datacontenttype")]
    public string DataContentType { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public ResumePayload Data { get; set; } = new();
}
