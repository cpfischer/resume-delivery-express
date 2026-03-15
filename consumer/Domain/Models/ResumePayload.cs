using System.Text.Json.Serialization;

namespace Consumer.Api.Domain.Services.Models;

public sealed class ResumePayload
{
    [JsonPropertyName("candidateName")]
    public string CandidateName { get; set; } = string.Empty;

    [JsonPropertyName("targetRole")]
    public string TargetRole { get; set; } = string.Empty;

    [JsonPropertyName("resumeText")]
    public string ResumeText { get; set; } = string.Empty;
}
