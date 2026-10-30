using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Slots;

/// <summary>The shift handover. Routed under the Slots slice, its views live in the slice's folder.</summary>
[Route("Slots/Handover")]
public sealed class HandoverController : Controller
{
    [HttpGet("")]
    public IActionResult Index(bool afresh) => View("/Slots/Handover.cshtml", new HandoverPage { Afresh = afresh });
}
