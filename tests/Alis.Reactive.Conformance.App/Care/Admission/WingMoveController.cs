using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WingMoveController : Controller
{
    [HttpGet(CarePages.WingMove)]
    public IActionResult ForAResident() => View("/Care/Admission/WingMove.cshtml", new WingMove { Wing = "Rose" });
}

public sealed class WingMove
{
    public string? Wing { get; set; }
}

public static class WingMoves
{
    public static readonly AppEvent MoveToOak = AppEvent.Declare();
}
