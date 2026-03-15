using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public sealed class SkillDetectorTests
{
    [TestMethod]
    public void Detect_ReturnsMatchedSkills_WhenKeywordsExist()
    {
        var text = "Worked with Kubernetes, rabbitmq, aws and Microservices.";

        var result = SkillDetector.Detect(text);

        CollectionAssert.Contains(result, "Kubernetes");
        CollectionAssert.Contains(result, "RabbitMQ");
        CollectionAssert.Contains(result, "AWS");
        CollectionAssert.Contains(result, "Microservices");
        CollectionAssert.DoesNotContain(result, "Grafana");
    }

    [TestMethod]
    public void Detect_ReturnsEmpty_WhenNoKeywordsExist()
    {
        var text = "Strong communicator and team collaborator.";

        var result = SkillDetector.Detect(text);

        Assert.AreEqual(0, result.Length);
    }
}
