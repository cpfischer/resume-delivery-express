using Microsoft.VisualStudio.TestTools.UnitTesting;
using Producer.Api.Application.Contracts;
using Producer.Api.Domain.Factories;

[TestClass]
public sealed class ResumeEventFactoryTests
{
    [TestMethod]
    public void Create_ReturnsCloudEventWithExpectedShape()
    {
        var eventId = "event-123";
        var request = new PublishResumeEventRequest();

        var cloudEvent = ResumeEventFactory.Create(eventId, request);

        Assert.AreEqual(eventId, cloudEvent.Id);
        Assert.AreEqual(request.Source, cloudEvent.Source);
        Assert.AreEqual(request.Type, cloudEvent.Type);
        Assert.AreEqual(request.SpecVersion, cloudEvent.SpecVersion);
        Assert.AreEqual(request.DataContentType, cloudEvent.DataContentType);
        Assert.AreEqual("Caleb Fischer", cloudEvent.Data.CandidateName);
        Assert.AreEqual("Software Engineer", cloudEvent.Data.TargetRole);
        StringAssert.Contains(cloudEvent.Data.ResumeText, "RabbitMQ");
    }
}
