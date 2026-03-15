using Microsoft.AspNetCore.Mvc;

[ApiController]
public sealed class RootController : ControllerBase
{
    [HttpGet("/")]
    public IActionResult RedirectToSwagger()
    {
        return Redirect("/swagger");
    }
}
