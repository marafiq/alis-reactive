using System.Text.Json;
using Alis.Reactive.SandboxApp.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline;

[Area("Sandbox")]
[Route("Sandbox/HttpPipeline/RealTime")]
public class RealTimeController : Controller
{
    private readonly IHubContext<NotificationHub> _notificationHub;
    private readonly IHubContext<ResidentStatusHub> _residentHub;

    public RealTimeController(
        IHubContext<NotificationHub> notificationHub,
        IHubContext<ResidentStatusHub> residentHub)
    {
        _notificationHub = notificationHub;
        _residentHub = residentHub;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public IActionResult Index() => View("~/Areas/Sandbox/Views/HttpPipeline/RealTime/Index.cshtml");

    [HttpGet("ResidentPanel")]
    public IActionResult ResidentPanel() => PartialView("~/Areas/Sandbox/Views/HttpPipeline/RealTime/_ResidentPanelPartial.cshtml");

    [HttpPost("PushNotification")]
    public async Task<IActionResult> PushNotification([FromBody] NotificationPayload payload)
    {
        await _notificationHub.Clients.All.SendAsync("ReceiveNotification", payload);
        return Ok();
    }

    [HttpPost("PushResidentStatus")]
    public async Task<IActionResult> PushResidentStatus([FromBody] ResidentStatusPayload payload)
    {
        await _residentHub.Clients.All.SendAsync("StatusChanged", payload);
        return Ok();
    }

    [HttpGet("/api/facility-alerts")]
    public async Task FacilityAlertStream(CancellationToken ct)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers.Connection = "keep-alive";

        var alert = JsonSerializer.Serialize(
            new { message = "Facility check complete", level = "info" },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        await Response.WriteAsync($"event: facility-alert\ndata: {alert}\n\n", ct);
        await Response.Body.FlushAsync(ct);

        try { await Task.Delay(Timeout.Infinite, ct); }
        catch (OperationCanceledException) { }
    }
}
