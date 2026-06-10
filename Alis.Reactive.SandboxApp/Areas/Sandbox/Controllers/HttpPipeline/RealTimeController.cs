using System.Collections.Concurrent;
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
    public IActionResult Index() => View("~/Areas/Sandbox/Views/HttpPipeline/RealTime/Index.cshtml");

    [HttpGet("ResidentPanel")]
    public IActionResult ResidentPanel(string? drill)
    {
        ViewData["DrillId"] = drill ?? "";
        return PartialView("~/Areas/Sandbox/Views/HttpPipeline/RealTime/_ResidentPanelPartial.cshtml");
    }

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
    public async Task FacilityAlertStream([FromQuery] string drill, CancellationToken ct)
    {
        var drillWorld = DrillWorldFor(drill);
        if (drillWorld.Broken)
        {
            // EventSource retries forever on 5xx but fails PERMANENTLY on 404 — permanent
            // failure is the state the runtime's retry indicator responds to.
            Response.StatusCode = 404;
            return;
        }

        Response.ContentType = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers.Connection = "keep-alive";

        var alert = JsonSerializer.Serialize(
            new { message = "Facility check complete", level = "info" },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var streamKey = Guid.NewGuid();
        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(ct);
        drillWorld.Streams[streamKey] = lifetime;
        try
        {
            // retry: 300 keeps the browser's reconnect delay short so outage drills observe
            // the permanent failure quickly instead of waiting on the ~3s browser default.
            await Response.WriteAsync($"retry: 300\nevent: facility-alert\ndata: {alert}\n\n", lifetime.Token);
            await Response.Body.FlushAsync(lifetime.Token);
            await Task.Delay(Timeout.Infinite, lifetime.Token);
        }
        catch (OperationCanceledException) { }
        finally { drillWorld.Streams.TryRemove(streamKey, out _); }
    }

    // Outage drill state is scoped per page load: the view embeds a fresh drill id in the
    // SSE URL and both drill buttons, so every page (and every test) breaks only its own
    // stream world. Nothing is process-global — a reload is always a fresh, healthy drill.
    private sealed class FacilityDrillWorld
    {
        public volatile bool Broken;
        public readonly ConcurrentDictionary<Guid, CancellationTokenSource> Streams = new();
    }

    private static readonly ConcurrentDictionary<string, FacilityDrillWorld> DrillWorlds = new();

    // A request without a drill id (stale page, direct curl) gets its own shared world
    // instead of a 500 — the empty-key world behaves exactly like any other drill.
    private static FacilityDrillWorld DrillWorldFor(string? drillId) =>
        DrillWorlds.GetOrAdd(drillId ?? "", _ => new FacilityDrillWorld());

    public record OutageDrillRequest(string DrillId);

    [HttpPost("BreakFacilityStream")]
    public IActionResult BreakFacilityStream([FromBody] OutageDrillRequest payload)
    {
        var drillWorld = DrillWorldFor(payload.DrillId);
        drillWorld.Broken = true;
        foreach (var stream in drillWorld.Streams.Values) stream.Cancel();
        return Ok();
    }

    [HttpPost("RestoreFacilityStream")]
    public IActionResult RestoreFacilityStream([FromBody] OutageDrillRequest payload)
    {
        DrillWorldFor(payload.DrillId).Broken = false;
        return Ok();
    }

    [HttpPost("BreakResidentHub")]
    public IActionResult BreakResidentHub([FromBody] OutageDrillRequest payload)
    {
        ResidentStatusHub.BreakDrill(payload.DrillId);
        return Ok();
    }

    [HttpPost("RestoreResidentHub")]
    public IActionResult RestoreResidentHub([FromBody] OutageDrillRequest payload)
    {
        ResidentStatusHub.RestoreDrill(payload.DrillId);
        return Ok();
    }
}
