using System.Text.Json.Serialization;

namespace Domain.Consumer.Models;

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

public sealed class ResumePayload
{
    [JsonPropertyName("candidateName")]
    public string CandidateName { get; set; } = string.Empty;

    [JsonPropertyName("targetRole")]
    public string TargetRole { get; set; } = string.Empty;

    [JsonPropertyName("resumeText")]
    public string ResumeText { get; set; } = string.Empty;
}

public sealed class ResumeResult
{
    public string EventId { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public IReadOnlyList<string> DetectedSkills { get; set; } = Array.Empty<string>();
    public DateTimeOffset ProcessedAt { get; set; }
    public string ProcessedByPod { get; set; } = string.Empty;
}
