using Producer.Api.Application.Contracts;
using Producer.Api.Domain.Factories;

namespace Producer.Api.Application.Services;

public sealed class ResumeEventApplicationService(IRabbitMqMessagePublisher rabbitMqMessagePublisher) : IResumeEventApplicationService
{
    public Task<string> PublishAsync(PublishResumeEventRequest request, CancellationToken cancellationToken)
    {
        var eventId = Guid.NewGuid().ToString();
        var cloudEvent = ResumeEventFactory.Create(eventId, request);
        rabbitMqMessagePublisher.Publish(cloudEvent);
        return Task.FromResult(cloudEvent.Id);
    }
}
