using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Producer.Api.Application.Services;
using Producer.Api.Application.Contracts;

[TestClass]
public sealed class ResultQueryServiceTests
{
    [TestMethod]
    public async Task GetResultAsync_ReturnsSuccess_WhenConsumerReturnsOk()
    {
        Environment.SetEnvironmentVariable("CONSUMER_RESULTS_BASE_URL", "http://consumer");
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"eventId\":\"e1\"}")
        };

        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(Arg.Any<string>())
            .Returns(_ => new HttpClient(new StaticResponseHandler(response), disposeHandler: false));
        var service = new ResultQueryService(httpClientFactory);

        var outcome = await service.GetResultAsync("e1", CancellationToken.None);

        Assert.AreEqual(ResultQueryStatus.Success, outcome.Status);
        Assert.AreEqual("{\"eventId\":\"e1\"}", outcome.JsonPayload);
        Assert.IsNull(outcome.Error);
    }

    [TestMethod]
    public async Task GetResultAsync_ReturnsNotFound_WhenConsumerReturns404()
    {
        Environment.SetEnvironmentVariable("CONSUMER_RESULTS_BASE_URL", "http://consumer");
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(Arg.Any<string>())
            .Returns(_ => new HttpClient(new StaticResponseHandler(new HttpResponseMessage(HttpStatusCode.NotFound)), disposeHandler: false));
        var service = new ResultQueryService(httpClientFactory);

        var outcome = await service.GetResultAsync("e404", CancellationToken.None);

        Assert.AreEqual(ResultQueryStatus.NotFound, outcome.Status);
        Assert.IsNull(outcome.JsonPayload);
    }

    [TestMethod]
    public async Task GetResultAsync_ReturnsFailed_WhenConsumerReturns500()
    {
        Environment.SetEnvironmentVariable("CONSUMER_RESULTS_BASE_URL", "http://consumer");
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(Arg.Any<string>())
            .Returns(_ => new HttpClient(new StaticResponseHandler(new HttpResponseMessage(HttpStatusCode.InternalServerError)), disposeHandler: false));
        var service = new ResultQueryService(httpClientFactory);

        var outcome = await service.GetResultAsync("e500", CancellationToken.None);

        Assert.AreEqual(ResultQueryStatus.Failed, outcome.Status);
        StringAssert.Contains(outcome.Error!, "500");
    }

    [TestMethod]
    public async Task GetResultAsync_ReturnsUnreachable_WhenHttpCallThrows()
    {
        Environment.SetEnvironmentVariable("CONSUMER_RESULTS_BASE_URL", "http://consumer");
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(Arg.Any<string>())
            .Returns(_ => new HttpClient(new ThrowingHandler(), disposeHandler: false));
        var service = new ResultQueryService(httpClientFactory);

        var outcome = await service.GetResultAsync("e-err", CancellationToken.None);

        Assert.AreEqual(ResultQueryStatus.Unreachable, outcome.Status);
        StringAssert.Contains(outcome.Error!, "/results/e-err");
    }

    private sealed class StaticResponseHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(response);
        }
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new HttpRequestException("boom");
        }
    }
}

