using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions;

[Area("Sandbox")]
[Route("Sandbox/Conditions/VitalsAlert")]
public class VitalsAlertController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return View("~/Areas/Sandbox/Views/Conditions/VitalsAlert/Index.cshtml", new VitalsAlertModel
        {
            HeartRate = 72
        });
    }

    /// <summary>Returns the high-rate alert response used by the condition-gated HTTP branch.</summary>
    [HttpPost("Alert")]
    public IActionResult Alert([FromBody] AlertRequest? request)
    {
        return Ok(new
        {
            message = $"Alert sent for HR {request?.HeartRate ?? 0}",
            timestamp = DateTime.UtcNow.ToString("HH:mm:ss"),
            level = "high"
        });
    }

    /// <summary>Returns the crisis-tier response for heart rates at or above 180.</summary>
    [HttpPost("Critical")]
    public IActionResult Critical([FromBody] AlertRequest? request)
    {
        return Ok(new
        {
            message = $"CRITICAL: HR {request?.HeartRate ?? 0} — code blue dispatched",
            timestamp = DateTime.UtcNow.ToString("HH:mm:ss"),
            level = "critical"
        });
    }

    /// <summary>Returns the elevated-tier response for heart rates from 140 through 179.</summary>
    [HttpPost("Warning")]
    public IActionResult Warning([FromBody] AlertRequest? request)
    {
        return Ok(new
        {
            message = $"WARNING: HR {request?.HeartRate ?? 0} — nurse notified",
            timestamp = DateTime.UtcNow.ToString("HH:mm:ss"),
            level = "warning"
        });
    }

    public class AlertRequest
    {
        public decimal HeartRate { get; set; }
    }
}
