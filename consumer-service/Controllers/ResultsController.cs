using Microsoft.AspNetCore.Mvc;

[ApiController]
public sealed class ResultsController(ResultStore store) : ControllerBase
{
    [HttpGet("/results/{eventId}")]
    public IActionResult GetResult([FromRoute] string eventId)
    {
        return store.TryGet(eventId, out var result)
            ? Ok(result)
            : NotFound();
    }
}
