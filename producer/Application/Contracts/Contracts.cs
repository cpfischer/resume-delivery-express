using Producer.Api.Domain.Factories;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Producer.Api.Application.Contracts;

public sealed record PublishEventResponse(string EventId);

public sealed class PublishResumeEventRequest
{
    [JsonPropertyName("id")]
    [DefaultValue("00000000-0000-0000-0000-000000000000")]
    public string Id { get; set; } = "00000000-0000-0000-0000-000000000000";

    [JsonPropertyName("source")]
    [DefaultValue("/producer/resume-events")]
    public string Source { get; set; } = "/producer/resume-events";

    [JsonPropertyName("type")]
    [DefaultValue("com.resume.submitted")]
    public string Type { get; set; } = "com.resume.submitted";

    [JsonPropertyName("specversion")]
    [DefaultValue("1.0")]
    public string SpecVersion { get; set; } = "1.0";

    [JsonPropertyName("time")]
    [DefaultValue("2026-03-15T21:00:00Z")]
    public DateTimeOffset Time { get; set; } = DateTimeOffset.Parse("2026-03-15T21:00:00Z");

    [JsonPropertyName("datacontenttype")]
    [DefaultValue("application/json")]
    public string DataContentType { get; set; } = "application/json";

    [JsonPropertyName("data")]
    public PublishResumeEventData Data { get; set; } = new();
}

public sealed class PublishResumeEventData
{
    [JsonPropertyName("candidateName")]
    [DefaultValue("Caleb Fischer")]
    public string CandidateName { get; set; } = "Caleb Fischer";

    [JsonPropertyName("targetRole")]
    [DefaultValue("Software Engineer")]
    public string TargetRole { get; set; } = "Software Engineer";

    [JsonPropertyName("resumeText")]
    [DefaultValue("Kubernetes RabbitMQ .NET AWS Grafana Microservices")]
    public string ResumeText { get; set; } = "Kubernetes RabbitMQ .NET AWS Grafana Microservices";
}

public sealed record HealthChecksResponse(string ProducerApi, string RabbitMq, string ConsumerResultsApi);

public sealed record HealthEndpointsResponse(string RabbitMq, string ConsumerResultsApi);

public sealed record HealthStatusResponse(
    string Status,
    bool AllConnectionsWorking,
    HealthChecksResponse Checks,
    HealthEndpointsResponse Endpoints);

public interface IResumeEventApplicationService
{
    Task<string> PublishAsync(PublishResumeEventRequest request, CancellationToken cancellationToken);
}

public interface IResultQueryService
{
    Task<ResultQueryOutcome> GetResultAsync(string eventId, CancellationToken cancellationToken);
}

public interface IHealthStatusService
{
    Task<HealthStatusResponse> GetStatusAsync(CancellationToken cancellationToken);
}

public interface IRabbitMqMessagePublisher
{
    void Publish(ProducerCloudEvent cloudEvent);
}

public enum ResultQueryStatus
{
    Success,
    NotFound,
    Failed,
    Unreachable
}

public sealed record ResultQueryOutcome(ResultQueryStatus Status, string? JsonPayload, string? Error);
