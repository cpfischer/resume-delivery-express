using Microsoft.VisualStudio.TestTools.UnitTesting;
using Producer.Api.Application.Services;
using Producer.Api.Application.Contracts;
using Producer.Api.Domain.Factories;

[TestClass]
public sealed class ResumeEventApplicationServiceTests
{
    [TestMethod]
    public async Task PublishAsync_PublishesCloudEvent_AndReturnsEventId()
    {
        var publisher = new CapturingPublisher();
        var service = new ResumeEventApplicationService(publisher);

        var eventId = await service.PublishAsync(new PublishResumeEventRequest(), CancellationToken.None);

        Assert.IsFalse(string.IsNullOrWhiteSpace(eventId));
        Assert.IsNotNull(publisher.LastPublishedEvent);
        Assert.AreEqual(eventId, publisher.LastPublishedEvent.Id);
        Assert.AreEqual("com.resume.submitted", publisher.LastPublishedEvent.Type);
    }

    private sealed class CapturingPublisher : IRabbitMqMessagePublisher
    {
        public ProducerCloudEvent LastPublishedEvent { get; private set; } = null!;

        public void Publish(ProducerCloudEvent cloudEvent)
        {
            LastPublishedEvent = cloudEvent;
        }
    }
}

