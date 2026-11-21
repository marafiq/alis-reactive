using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class KinMoveInController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.KinMoveIn)]
    public IActionResult OfAResident() => View("/Care/Admission/KinMoveIn.cshtml", new KinMoveIn());

    [HttpPost(KinMoveInEndpoints.MoveIns)]
    public IActionResult Save([FromForm] KinMoveIn moveIn)
        => Content($"next of kin {Bound(moveIn.NextOfKin)} and alternate {Bound(moveIn.Alternate)}", PlainText);

    private static string Bound(KinContact contact) => contact.Name is { } name ? $"{name} {contact.Phone}".TrimEnd() : "none";
}

public sealed class KinMoveIn
{
    public KinContact NextOfKin { get; set; } = new();

    public KinContact Alternate { get; set; } = new();
}

public sealed class KinContact
{
    public string? Name { get; set; }

    public string? Phone { get; set; }
}

public static class KinMoveInEndpoints
{
    public const string MoveIns = CarePages.Area + "admission/kin-move-in/move-ins";
}

public static class KinMoveIns
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
