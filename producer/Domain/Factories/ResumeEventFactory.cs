namespace Producer.Api.Domain.Factories;

public static class ResumeEventFactory
{
    public static ProducerCloudEvent Create(string eventId)
    {
        return new ProducerCloudEvent
        {
            Id = eventId,
            Source = "/producer/resume-events",
            Type = "com.resume.submitted",
            SpecVersion = "1.0",
            Time = DateTimeOffset.UtcNow,
            DataContentType = "application/json",
            Data = new ProducerResumePayload
            {
                CandidateName = "Caleb Fischer",
                TargetRole = "Software Engineer",
                ResumeText = "Kubernetes RabbitMQ .NET AWS Grafana Microservices"
            }
        };
    }
}

public sealed class ProducerCloudEvent
{
    public string Id { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string SpecVersion { get; set; } = string.Empty;
    public DateTimeOffset Time { get; set; }
    public string DataContentType { get; set; } = string.Empty;
    public ProducerResumePayload Data { get; set; } = new();
}

public sealed class ProducerResumePayload
{
    public string CandidateName { get; set; } = string.Empty;
    public string TargetRole { get; set; } = string.Empty;
    public string ResumeText { get; set; } = string.Empty;
}
