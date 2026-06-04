using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthCheckController : ControllerBase
{
    
    [HttpGet("ping")]
    public IActionResult Ping() 
        => Ok(new
        {
            Success = true,
            Message = "Pong",
            Date = DateTime.Now.ToString(CultureInfo.InvariantCulture)
        });
}