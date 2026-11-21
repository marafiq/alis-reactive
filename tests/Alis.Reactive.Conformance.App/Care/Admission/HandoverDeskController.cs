using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class HandoverDeskController : Controller
{
    [HttpGet(CarePages.HandoverDesk)]
    public IActionResult OfTheEvening() => View("/Care/Admission/HandoverDesk.cshtml", new HandoverDesk("Ada Hart"));

    [HttpGet(HandoverDeskEndpoints.First)]
    public IActionResult First() => Json(new HandoverSeen(new DateTimeOffset(2026, 11, 9, 10, 0, 0, TimeSpan.FromHours(-8))));

    [HttpGet(HandoverDeskEndpoints.Second)]
    public IActionResult Second() => Json(new HandoverSeen(new DateTimeOffset(2026, 11, 9, 10, 0, 0, TimeSpan.FromHours(-7))));
}

public sealed record HandoverDesk(string Resident);

public sealed record HandoverSeen(DateTimeOffset Seen);

public static class HandoverDeskEndpoints
{
    public const string First = CarePages.HandoverDesk + "/first";

    public const string Second = CarePages.HandoverDesk + "/second";
}

public static class HandoverDesks
{
    public static readonly AppEvent LoadTheFirst = AppEvent.Declare();

    public static readonly AppEvent LoadTheSecond = AppEvent.Declare();
}
