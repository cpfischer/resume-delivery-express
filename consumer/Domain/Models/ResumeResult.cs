namespace Consumer.Api.Domain.Services.Models;

public sealed class ResumeResult
{
    public string EventId { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public IReadOnlyList<string> DetectedSkills { get; set; } = Array.Empty<string>();
    public DateTimeOffset ProcessedAt { get; set; }
    public string ProcessedByPod { get; set; } = string.Empty;
}
