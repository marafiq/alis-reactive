using Alis.Reactive.SandboxApp.RealTime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline;

[Area("Sandbox")]
[Route("Sandbox/HttpPipeline/RealTime")]
public class RealTimeController : Controller
{
    private readonly IHubContext<NotificationHub, INotificationClient> _notificationHub;
    private readonly IHubContext<ResidentStatusHub, IResidentStatusClient> _residentHub;

    public RealTimeController(
        IHubContext<NotificationHub, INotificationClient> notificationHub,
        IHubContext<ResidentStatusHub, IResidentStatusClient> residentHub)
    {
        _notificationHub = notificationHub;
        _residentHub = residentHub;
    }

    [HttpGet("")]
    public IActionResult Index() => View("~/Areas/Sandbox/Views/HttpPipeline/RealTime/Index.cshtml");

    [HttpGet("ResidentPanel")]
    public IActionResult ResidentPanel() => PartialView("~/Areas/Sandbox/Views/HttpPipeline/RealTime/_ResidentPanelPartial.cshtml");

    [HttpPost("PushNotification")]
    public async Task<IActionResult> PushNotification([FromBody] NotificationPayload payload)
    {
        await _notificationHub.Clients.All.ReceiveNotification(payload);
        return Ok();
    }

    [HttpPost("PushResidentStatus")]
    public async Task<IActionResult> PushResidentStatus([FromBody] ResidentStatusPayload payload)
    {
        await _residentHub.Clients.All.StatusChanged(payload);
        return Ok();
    }
}
