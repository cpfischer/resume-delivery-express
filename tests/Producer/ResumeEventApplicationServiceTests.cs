using Microsoft.VisualStudio.TestTools.UnitTesting;
using Application.Producer.Contracts;
using Application.Producer.Services;
using Domain.Producer.Events;

[TestClass]
public sealed class ResumeEventApplicationServiceTests
{
    [TestMethod]
    public async Task PublishAsync_PublishesCloudEvent_AndReturnsEventId()
    {
        var publisher = new CapturingPublisher();
        var service = new ResumeEventApplicationService(publisher);

        var eventId = await service.PublishAsync(CancellationToken.None);

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
