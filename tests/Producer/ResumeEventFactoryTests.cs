using Microsoft.VisualStudio.TestTools.UnitTesting;
using Domain.Producer.Events;

[TestClass]
public sealed class ResumeEventFactoryTests
{
    [TestMethod]
    public void Create_ReturnsCloudEventWithExpectedShape()
    {
        var eventId = "event-123";

        var cloudEvent = ResumeEventFactory.Create(eventId);

        Assert.AreEqual(eventId, cloudEvent.Id);
        Assert.AreEqual("/producer/resume-events", cloudEvent.Source);
        Assert.AreEqual("com.resume.submitted", cloudEvent.Type);
        Assert.AreEqual("1.0", cloudEvent.SpecVersion);
        Assert.AreEqual("application/json", cloudEvent.DataContentType);
        Assert.AreEqual("Caleb Fischer", cloudEvent.Data.CandidateName);
        Assert.AreEqual("Software Engineer", cloudEvent.Data.TargetRole);
        StringAssert.Contains(cloudEvent.Data.ResumeText, "RabbitMQ");
    }
}
