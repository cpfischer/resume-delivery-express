using Microsoft.AspNetCore.Mvc;
using Application.Producer.Contracts;

namespace Producer.Api;

[ApiController]
public sealed class ResumeEventsController(IResumeEventApplicationService resumeEventApplicationService, IResultQueryService resultQueryService) : ControllerBase
{
    [HttpPost("/publish-resume-event")]
    public async Task<ActionResult<PublishEventResponse>> PublishResumeEvent(CancellationToken cancellationToken)
    {
        var eventId = await resumeEventApplicationService.PublishAsync(cancellationToken);
        return Ok(new PublishEventResponse(eventId));
    }

    [HttpGet("/results/{eventId}")]
    public async Task<IActionResult> GetResult([FromRoute] string eventId, CancellationToken cancellationToken)
    {
        var outcome = await resultQueryService.GetResultAsync(eventId, cancellationToken);

        if (outcome.Status == ResultQueryStatus.NotFound)
        {
            return NotFound();
        }

        if (outcome.Status == ResultQueryStatus.Unreachable)
        {
            return Problem(
                title: "Consumer service is unreachable",
                detail: outcome.Error,
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        if (outcome.Status == ResultQueryStatus.Failed)
        {
            return Problem(
                title: "Consumer service returned an error",
                detail: outcome.Error,
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        return Content(outcome.JsonPayload!, "application/json");
    }
}
