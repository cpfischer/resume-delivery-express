using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Producer.Api.Api;
using Producer.Api.Application.Contracts;

[TestClass]
public sealed class ProducerControllersTests
{
    [TestMethod]
    public async Task ResumeEventsController_PublishResumeEvent_ReturnsOkWithEventId()
    {
        var resumeEventService = Substitute.For<IResumeEventApplicationService>();
        resumeEventService.PublishAsync(Arg.Any<CancellationToken>()).Returns("evt-1");
        var resultQueryService = Substitute.For<IResultQueryService>();
        var controller = new ResumeEventsController(
            resumeEventService,
            resultQueryService);

        var actionResult = await controller.PublishResumeEvent(CancellationToken.None);

        var okResult = actionResult.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var payload = okResult.Value as PublishEventResponse;
        Assert.IsNotNull(payload);
        Assert.AreEqual("evt-1", payload.EventId);
    }

    [TestMethod]
    public async Task ResumeEventsController_GetResult_ReturnsNotFound_WhenServiceSaysNotFound()
    {
        var resultQueryService = Substitute.For<IResultQueryService>();
        resultQueryService.GetResultAsync("evt-1", Arg.Any<CancellationToken>())
            .Returns(new ResultQueryOutcome(ResultQueryStatus.NotFound, null, null));
        var controller = new ResumeEventsController(
            Substitute.For<IResumeEventApplicationService>(),
            resultQueryService);

        var result = await controller.GetResult("evt-1", CancellationToken.None);

        Assert.IsInstanceOfType<NotFoundResult>(result);
    }

    [TestMethod]
    public async Task ResumeEventsController_GetResult_ReturnsProblem_WhenUnreachable()
    {
        var resultQueryService = Substitute.For<IResultQueryService>();
        resultQueryService.GetResultAsync("evt-1", Arg.Any<CancellationToken>())
            .Returns(new ResultQueryOutcome(ResultQueryStatus.Unreachable, null, "cannot reach consumer"));
        var controller = new ResumeEventsController(
            Substitute.For<IResumeEventApplicationService>(),
            resultQueryService);

        var result = await controller.GetResult("evt-1", CancellationToken.None);

        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(503, objectResult.StatusCode);
    }

    [TestMethod]
    public async Task ResumeEventsController_GetResult_ReturnsContent_WhenSuccess()
    {
        var resultQueryService = Substitute.For<IResultQueryService>();
        resultQueryService.GetResultAsync("evt-1", Arg.Any<CancellationToken>())
            .Returns(new ResultQueryOutcome(ResultQueryStatus.Success, "{\"ok\":true}", null));
        var controller = new ResumeEventsController(
            Substitute.For<IResumeEventApplicationService>(),
            resultQueryService);

        var result = await controller.GetResult("evt-1", CancellationToken.None);

        var contentResult = result as ContentResult;
        Assert.IsNotNull(contentResult);
        Assert.AreEqual("application/json", contentResult.ContentType);
        Assert.AreEqual("{\"ok\":true}", contentResult.Content);
    }

    [TestMethod]
    public async Task HealthController_GetHealth_ReturnsOkPayload()
    {
        var expected = new HealthStatusResponse(
            "degraded",
            false,
            new HealthChecksResponse("reachable", "unreachable", "unreachable"),
            new HealthEndpointsResponse("rabbitmq:5672", "consumer:5002"));
        var healthService = Substitute.For<IHealthStatusService>();
        healthService.GetStatusAsync(Arg.Any<CancellationToken>()).Returns(expected);
        var controller = new HealthController(healthService);

        var actionResult = await controller.GetHealth(CancellationToken.None);

        var okResult = actionResult.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreSame(expected, okResult.Value);
    }

    [TestMethod]
    public void RootController_RedirectToSwagger_ReturnsRedirectResult()
    {
        var controller = new RootController();

        var result = controller.RedirectToSwagger();

        var redirect = result as RedirectResult;
        Assert.IsNotNull(redirect);
        Assert.AreEqual("/swagger", redirect.Url);
    }

}


