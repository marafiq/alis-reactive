using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class MoveInController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(RefusalPages.MoveInPrefixAfterItsPlan)]
    public IActionResult PrefixAfterItsPlan() => View("/Refusals/InputsAndIds/MoveInPrefixAfterItsPlan.cshtml", new Kin());

    [HttpGet(RefusalPages.MoveInPrefixBeforeItsPlan)]
    public IActionResult PrefixBeforeItsPlan() => View("/Refusals/InputsAndIds/MoveInPrefixBeforeItsPlan.cshtml", new Kin());

    [HttpPost(RefusalEndpoints.MoveIns)]
    public IActionResult Save([FromForm] MoveIn moveIn)
        => Content(moveIn.NextOfKin.Name is { } name ? $"Next of kin: {name}" : "No next of kin", PlainText);
}

public sealed class MoveIn
{
    public Kin NextOfKin { get; set; } = new();
}

public sealed class Kin
{
    public string? Name { get; set; }
}

public static class MoveIns
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
