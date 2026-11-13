using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BedMoveController : Controller
{
    [HttpGet(CarePages.BedMove)]
    public IActionResult OfAResident() => View("/Care/BedBoard/BedMove.cshtml", new BedMove { Ward = ["north-2"], Wings = Wings() });

    [HttpGet(CarePages.BedMoveUnplaced)]
    public IActionResult OfAnUnplacedResident() => View("/Care/BedBoard/BedMove.cshtml", new BedMove { Ward = null, Wings = Wings() });

    private static IReadOnlyList<WingNode> Wings()
    {
        var north = new WingNode("north", "North wing", [new WingNode("north-1", "North 1", null), new WingNode("north-2", "North 2", null)]);
        var south = new WingNode("south", "South wing", [new WingNode("south-1", "South 1", null)]);
        return [north, south];
    }
}

public sealed class BedMove
{
    public string[]? Ward { get; set; }

    public IReadOnlyList<WingNode> Wings { get; init; } = [];
}

public sealed record WingNode(string Id, string Name, IReadOnlyList<WingNode>? Wards);
