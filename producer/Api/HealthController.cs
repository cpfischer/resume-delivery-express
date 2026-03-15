using Microsoft.AspNetCore.Mvc;
using Producer.Api.Application.Contracts;

namespace Producer.Api.Api;

[ApiController]
public sealed class HealthController(IHealthStatusService healthStatusService) : ControllerBase
{
    [HttpGet("/health")]
    public async Task<ActionResult<HealthStatusResponse>> GetHealth(CancellationToken cancellationToken)
    {
        var result = await healthStatusService.GetStatusAsync(cancellationToken);
        return result.AllConnectionsWorking
            ? Ok(result)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, result);
    }
}
