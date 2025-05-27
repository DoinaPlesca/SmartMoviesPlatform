using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MovieService.FeatureToogles;

namespace MovieService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeatureFlagsController : ControllerBase
{
    private readonly IOptions<FeatureFlags> _featureFlags;

    public FeatureFlagsController(IOptions<FeatureFlags> featureFlags)
    {
        _featureFlags = featureFlags;
    }

    [HttpGet]
    public IActionResult GetFlags()
    {
        return Ok(_featureFlags.Value);
    }
}