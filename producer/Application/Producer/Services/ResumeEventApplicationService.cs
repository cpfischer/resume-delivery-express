using Application.Producer.Contracts;
using Domain.Producer.Events;

namespace Application.Producer.Services;

public sealed class ResumeEventApplicationService(IRabbitMqMessagePublisher rabbitMqMessagePublisher) : IResumeEventApplicationService
{
    public Task<string> PublishAsync(CancellationToken cancellationToken)
    {
        var eventId = Guid.NewGuid().ToString();
        var cloudEvent = ResumeEventFactory.Create(eventId);
        rabbitMqMessagePublisher.Publish(cloudEvent);
        return Task.FromResult(eventId);
    }
}
