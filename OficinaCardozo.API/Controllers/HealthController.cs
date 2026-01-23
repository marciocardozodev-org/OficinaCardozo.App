using Microsoft.AspNetCore.Mvc;
using OficinaCardozo.Application.Interfaces;

namespace OficinaCardozo.API.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly IHealthService _healthService;

    public HealthController(IHealthService healthService)
    {
        _healthService = healthService;
        Console.WriteLine($"[HealthController] Instanciado com IHealthService em {DateTime.UtcNow:O}");
    }

    [HttpGet("live")]
    public IActionResult Live()
    {
        Console.WriteLine($"[HealthController] Live endpoint chamado em {DateTime.UtcNow:O}");
        var dbHealthy = _healthService.IsDatabaseHealthy();
        return Ok(new { status = "Live", dbHealthy });
    }
}