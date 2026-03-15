using Microsoft.AspNetCore.Mvc;
using Application.Producer.Contracts;

namespace Producer.Api;

[ApiController]
public sealed class HealthController(IHealthStatusService healthStatusService) : ControllerBase
{
    [HttpGet("/health")]
    public async Task<ActionResult<HealthStatusResponse>> GetHealth(CancellationToken cancellationToken)
    {
        var result = await healthStatusService.GetStatusAsync(cancellationToken);
        return Ok(result);
    }
}
