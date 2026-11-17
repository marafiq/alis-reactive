using System.IO;
using System.Threading.Tasks;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class NightHandoverController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.NightHandoverWithANote)]
    public IActionResult WithANote() => View("/Care/Round/NightHandover.cshtml", new NightHandoverDesk { Resident = "Ada Hart", Handover = NightHandovers.Noted });

    [HttpGet(CarePages.NightHandoverWithNoNote)]
    public IActionResult WithNoNote() => View("/Care/Round/NightHandover.cshtml", new NightHandoverDesk { Resident = "Ben Okafor", Handover = NightHandovers.Unnoted });

    [HttpGet(NightHandovers.Noted)]
    public IActionResult Noted() => Json(new NightHandoverReply { Note = "Slept well" });

    [HttpGet(NightHandovers.Unnoted)]
    public IActionResult Unnoted() => Json(new NightHandoverReply());

    [HttpPost(NightHandovers.Received)]
    public async Task<IActionResult> Received()
    {
        using var reader = new StreamReader(Request.Body);
        var note = await reader.ReadToEndAsync();
        return Content(note.Length == 0 ? "Handed over with no note" : "Handed over: " + note, PlainText);
    }
}

public sealed class NightHandoverDesk
{
    public string Resident { get; init; } = "";

    public string Handover { get; init; } = "";
}

public sealed class NightHandoverReply
{
    public string? Note { get; init; }
}

public static class NightHandovers
{
    private const string Root = CarePages.Area + "round/night-handover/";

    public const string Noted = Root + "noted";

    public const string Unnoted = Root + "unnoted";

    public const string Received = Root + "received";

    public static readonly AppEvent HandOver = AppEvent.Declare();
}
