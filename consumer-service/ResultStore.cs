using System.Collections.Concurrent;

public sealed class ResultStore
{
    private readonly ConcurrentDictionary<string, ResumeResult> _results = new();

    public void Save(ResumeResult result)
    {
        _results[result.EventId] = result;
    }

    public bool TryGet(string eventId, out ResumeResult? result)
    {
        return _results.TryGetValue(eventId, out result);
    }
}
