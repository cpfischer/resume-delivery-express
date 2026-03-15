using Domain.Producer.Events;

namespace Application.Producer.Contracts;

public sealed record PublishEventResponse(string EventId);

public sealed record HealthChecksResponse(string ProducerApi, string RabbitMq, string ConsumerResultsApi);

public sealed record HealthEndpointsResponse(string RabbitMq, string ConsumerResultsApi);

public sealed record HealthStatusResponse(
    string Status,
    bool AllConnectionsWorking,
    HealthChecksResponse Checks,
    HealthEndpointsResponse Endpoints);

public interface IResumeEventApplicationService
{
    Task<string> PublishAsync(CancellationToken cancellationToken);
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
