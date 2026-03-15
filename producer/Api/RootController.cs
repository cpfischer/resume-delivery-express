using Microsoft.AspNetCore.Mvc;

namespace Producer.Api;

[ApiController]
public sealed class RootController : ControllerBase
{
    [HttpGet("/")]
    public IActionResult RedirectToSwagger()
    {
        return Redirect("/swagger");
    }
}
