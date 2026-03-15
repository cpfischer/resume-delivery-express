using Microsoft.AspNetCore.Mvc;

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
