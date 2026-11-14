using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BedsideBellController : Controller
{
    [HttpGet(CarePages.BedsideBellUnderReplace)]
    public IActionResult UnderReplace() => View("/Care/Record/BedsideBell.cshtml", new BedsideBell("Ada Hart", HandlerPolicy.Replace));

    [HttpGet(CarePages.BedsideBellUnderOverlap)]
    public IActionResult UnderOverlap() => View("/Care/Record/BedsideBell.cshtml", new BedsideBell("Ada Hart", HandlerPolicy.Overlap));

    [HttpGet(BedsideBellEndpoints.Desk)]
    public IActionResult Desk() => NoContent();
}

public sealed record BedsideBell(string Resident, HandlerPolicy Policy);

public sealed class BedsideRing
{
    public int Round { get; set; }
}

public static class BedsideBellEndpoints
{
    public const string Desk = CarePages.Area + "record/bedside-bell/desk";
}

public static class BedsideBells
{
    public static readonly AppEvent<BedsideRing> Rang = AppEvent.Declare<BedsideRing>();
}
