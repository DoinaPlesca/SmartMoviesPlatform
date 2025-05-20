using KongManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace KongManager.Controllers;

[ApiController]
[Route("kong")]
public class KongController : ControllerBase
{
    private readonly KongAdminService _kong;

    public KongController(KongAdminService kong)
    {
        _kong = kong;
    }

    [HttpPost("register-service")]
    public async Task<IActionResult> RegisterService([FromQuery] string name, [FromQuery] string url)
    {
        var result = await _kong.RegisterServiceAsync(name, url);
        return Ok(result);
    }

    [HttpPost("register-route")]
    public async Task<IActionResult> RegisterRoute([FromQuery] string serviceName, [FromQuery] string path)
    {
        var result = await _kong.RegisterRouteAsync(serviceName, path);
        return Ok(result);
    }
}