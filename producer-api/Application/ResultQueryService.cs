using System.Net;

public sealed class ResultQueryService(IHttpClientFactory httpClientFactory) : IResultQueryService
{
    public async Task<ResultQueryOutcome> GetResultAsync(string eventId, CancellationToken cancellationToken)
    {
        var consumerBaseUrl = Environment.GetEnvironmentVariable("CONSUMER_RESULTS_BASE_URL") ?? "http://localhost:5002";
        var requestUrl = $"{consumerBaseUrl.TrimEnd('/')}/results/{eventId}";

        try
        {
            var httpClient = httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(requestUrl, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new ResultQueryOutcome(ResultQueryStatus.NotFound, null, null);
            }

            if (!response.IsSuccessStatusCode)
            {
                return new ResultQueryOutcome(
                    ResultQueryStatus.Failed,
                    null,
                    $"Consumer response code: {(int)response.StatusCode}");
            }

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return new ResultQueryOutcome(ResultQueryStatus.Success, responseBody, null);
        }
        catch (Exception)
        {
            return new ResultQueryOutcome(ResultQueryStatus.Unreachable, null, $"Could not call {requestUrl}");
        }
    }
}
