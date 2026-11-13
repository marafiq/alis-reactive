using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class EscortDeskController : Controller
{
    [HttpGet(CarePages.EscortDesk)]
    public IActionResult OfTheRecord() => View("/Care/Record/EscortDesk.cshtml", new EscortDesk("Ada Hart"));
}

public sealed record EscortDesk(string Resident);

public sealed class EscortRequest
{
    public string? Guide { get; set; } = "the porter";

    public string? Note { get; set; }
}

public static class EscortDesks
{
    public static readonly AppEvent<EscortRequest> Requested = AppEvent.Declare<EscortRequest>();

    public static readonly AppEvent SendAgain = AppEvent.Declare();
}
