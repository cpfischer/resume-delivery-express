using System.Collections.Concurrent;
using Consumer.Api.Domain.Services.Models;

namespace Consumer.Api.Infrastructure.Persistence;

public sealed class ResultStore
{
    private readonly ConcurrentDictionary<string, ResumeResult> results = new();

    public void Save(ResumeResult result)
    {
        results[result.EventId] = result;
    }

    public bool TryGet(string eventId, out ResumeResult? result)
    {
        return results.TryGetValue(eventId, out result);
    }
}
