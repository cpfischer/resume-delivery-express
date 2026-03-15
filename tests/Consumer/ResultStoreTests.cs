using Microsoft.VisualStudio.TestTools.UnitTesting;
using Domain.Consumer.Models;
using Infrastructure.Consumer.Persistence;

[TestClass]
public sealed class ResultStoreTests
{
    [TestMethod]
    public void Save_ThenTryGet_ReturnsStoredResult()
    {
        var store = new ResultStore();
        var result = new ResumeResult
        {
            EventId = "evt-1",
            CandidateName = "Test User",
            DetectedSkills = [".NET"],
            ProcessedAt = DateTimeOffset.UtcNow,
            ProcessedByPod = "pod-abc"
        };

        store.Save(result);

        var found = store.TryGet("evt-1", out var stored);

        Assert.IsTrue(found);
        Assert.IsNotNull(stored);
        Assert.AreEqual("Test User", stored.CandidateName);
        Assert.AreEqual("pod-abc", stored.ProcessedByPod);
    }
}
