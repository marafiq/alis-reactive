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

    /// <summary>
    /// Simple alert — called when heart rate exceeds threshold.
    /// Returns confirmation with server timestamp.
    /// </summary>
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

    /// <summary>
    /// Critical alert — called when heart rate >= 180 (crisis tier).
    /// </summary>
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

    /// <summary>
    /// Warning alert — called when heart rate 140–179 (elevated tier).
    /// </summary>
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
