using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
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
        await _notificationHub.Clients.All.SendAsync(new NotificationFeed().ReceiveNotification.Name, payload);
        return Ok();
    }

    [HttpGet("ResidentPanelClosed")]
    public IActionResult ResidentPanelClosed()
        => PartialView("~/Areas/Sandbox/Views/HttpPipeline/RealTime/_ResidentPanelClosed.cshtml");

    [HttpPost("PushResidentStatus")]
    public async Task<IActionResult> PushResidentStatus([FromBody] ResidentStatusPayload payload, [FromQuery] string? drill)
    {
        payload.Connections = ResidentStatusHub.ConnectionsOf(drill ?? "").Count;
        await _residentHub.Clients.All.SendAsync(new ResidentStatusFeed().StatusChanged.Name, payload);
        return Ok();
    }

    [HttpGet("/api/facility-alerts")]
    public async Task FacilityAlertStream([FromQuery] string drill, CancellationToken ct)
    {
        var drillWorld = DrillWorldFor(drill);
        if (drillWorld.Broken)
        {
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
        var outbox = Channel.CreateUnbounded<string>();
        drillWorld.Streams[streamKey] = lifetime;
        drillWorld.Outboxes[streamKey] = outbox;
        try
        {
            await Response.WriteAsync($"retry: 300\nevent: {new FacilityAlertStream().Alert.Name}\ndata: {alert}\n\n", lifetime.Token);
            await Response.Body.FlushAsync(lifetime.Token);
            await foreach (var message in outbox.Reader.ReadAllAsync(lifetime.Token))
            {
                await Response.WriteAsync(message, lifetime.Token);
                await Response.Body.FlushAsync(lifetime.Token);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            drillWorld.Streams.TryRemove(streamKey, out _);
            drillWorld.Outboxes.TryRemove(streamKey, out _);
        }
    }

    [HttpPost("SoundAllClear")]
    public IActionResult SoundAllClear([FromBody] OutageDrillRequest payload)
    {
        foreach (var outbox in DrillWorldFor(payload.DrillId).Outboxes.Values)
            outbox.Writer.TryWrite($"event: {new FacilityAlertStream().AllClear.Name}\ndata: {{}}\n\n");
        return Ok();
    }

    private sealed class FacilityDrillWorld
    {
        public volatile bool Broken;
        public readonly ConcurrentDictionary<Guid, CancellationTokenSource> Streams = new();
        public readonly ConcurrentDictionary<Guid, Channel<string>> Outboxes = new();
    }

    private static readonly ConcurrentDictionary<string, FacilityDrillWorld> DrillWorlds = new();

    private static FacilityDrillWorld DrillWorldFor(string? drillId)
        => DrillWorlds.GetOrAdd(drillId ?? "", _ => new FacilityDrillWorld());

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
