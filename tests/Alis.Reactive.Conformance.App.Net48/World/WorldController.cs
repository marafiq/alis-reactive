using System.Net;
using System.Text.Json;
using System.Web.Mvc;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.World;

public sealed class WorldController : Controller
{
    private const string JsonText = "application/json";
    private const string PlainText = "text/plain";

    [HttpPost]
    [Address(WorldEndpoints.Seed)]
    public ActionResult Seed()
    {
        var seed = JsonSerializer.Deserialize<Seed>(Request.InputStream, TwinWorlds.HarnessJson) ?? throw new InvalidOperationException("The seed's body holds no seed.");
        TwinWorlds.Of(HttpContext).Take(seed, NoticeFolderOf(TwinWorlds.KeyOf(HttpContext)));
        return new EmptyResult();
    }

    [HttpPost]
    [Address(WorldEndpoints.End)]
    public ActionResult End()
    {
        var key = TwinWorlds.KeyOf(HttpContext);
        TwinWorlds.End(key);
        var noticeFolder = NoticeFolderOf(key);
        if (Directory.Exists(noticeFolder)) Directory.Delete(noticeFolder, recursive: true);
        return Content(JsonSerializer.Serialize(new WorldEnd(), TwinWorlds.HarnessJson), JsonText);
    }

    [HttpPost]
    [Address(WorldEndpoints.HostOrders)]
    public async Task<ActionResult> HostOrders()
        => await TwinWorlds.Of(HttpContext).HostOrders.Next(Response.ClientDisconnectedToken) is { } elementId
            ? Content(elementId, PlainText)
            : new HttpStatusCodeResult(HttpStatusCode.NoContent);

    [HttpPost]
    [Address(WorldEndpoints.HostRestart)]
    public async Task<ActionResult> HostRestart()
    {
        var order = JsonSerializer.Deserialize<RestartOrder>(Request.InputStream, TwinWorlds.HarnessJson) ?? throw new InvalidOperationException("The restart's body holds no order.");
        var report = await TwinWorlds.Of(HttpContext).HostOrders.Restart(order.ElementId, Response.ClientDisconnectedToken);
        return report.Length > 0 ? Conflict(report) : new HttpStatusCodeResult(HttpStatusCode.OK);
    }

    [HttpPost]
    [Address(WorldEndpoints.HostDone)]
    public ActionResult HostDone()
    {
        using var body = new StreamReader(Request.InputStream);
        return TwinWorlds.Of(HttpContext).HostOrders.Done(body.ReadToEnd()) ? new HttpStatusCodeResult(HttpStatusCode.OK) : Conflict("No restart is in work in this world.");
    }

    private ContentResult Conflict(string report)
    {
        Response.StatusCode = (int)HttpStatusCode.Conflict;
        Response.TrySkipIisCustomErrors = true;
        return Content(report, PlainText);
    }

    private string NoticeFolderOf(string key) => Server.MapPath("~/Content/World/" + key);
}
