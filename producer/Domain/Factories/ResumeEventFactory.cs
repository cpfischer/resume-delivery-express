using Producer.Api.Application.Contracts;
using System.Text.Json.Serialization;

namespace Producer.Api.Domain.Factories;

public static class ResumeEventFactory
{
    public static ProducerCloudEvent Create(string eventId, PublishResumeEventRequest request)
    {
        return new ProducerCloudEvent
        {
            Id = eventId,
            Source = string.IsNullOrWhiteSpace(request.Source) ? "/producer/resume-events" : request.Source,
            Type = string.IsNullOrWhiteSpace(request.Type) ? "com.resume.submitted" : request.Type,
            SpecVersion = string.IsNullOrWhiteSpace(request.SpecVersion) ? "1.0" : request.SpecVersion,
            Time = request.Time == default ? DateTimeOffset.UtcNow : request.Time,
            DataContentType = string.IsNullOrWhiteSpace(request.DataContentType) ? "application/json" : request.DataContentType,
            Data = new ProducerResumePayload
            {
                CandidateName = request.Data.CandidateName,
                TargetRole = request.Data.TargetRole,
                ResumeText = request.Data.ResumeText
            }
        };
    }
}

public sealed class ProducerCloudEvent
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
    public ProducerResumePayload Data { get; set; } = new();
}

public sealed class ProducerResumePayload
{
    [JsonPropertyName("candidateName")]
    public string CandidateName { get; set; } = string.Empty;
    [JsonPropertyName("targetRole")]
    public string TargetRole { get; set; } = string.Empty;
    [JsonPropertyName("resumeText")]
    public string ResumeText { get; set; } = string.Empty;
}
