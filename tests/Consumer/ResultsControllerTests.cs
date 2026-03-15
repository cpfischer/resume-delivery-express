using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Consumer.Api;
using Consumer.Api.Domain.Services.Models;
using Consumer.Api.Infrastructure.Persistence;

[TestClass]
public sealed class ResultsControllerTests
{
    [TestMethod]
    public void GetResult_ReturnsOk_WhenResultExists()
    {
        var store = new ResultStore();
        store.Save(new ResumeResult
        {
            EventId = "evt-123",
            CandidateName = "Caleb",
            DetectedSkills = ["Kubernetes"],
            ProcessedAt = DateTimeOffset.UtcNow,
            ProcessedByPod = "pod-a"
        });

        var controller = new ResultsController(store);

        var result = controller.GetResult("evt-123");

        Assert.IsInstanceOfType<OkObjectResult>(result);
    }

    [TestMethod]
    public void GetResult_ReturnsNotFound_WhenResultDoesNotExist()
    {
        var controller = new ResultsController(new ResultStore());

        var result = controller.GetResult("missing");

        Assert.IsInstanceOfType<NotFoundResult>(result);
    }
}

