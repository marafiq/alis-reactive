using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class LinenCountController : Controller
{
    [HttpGet(CarePages.LinenCount)]
    public IActionResult OfTheWard() => View("/Care/BedBoard/LinenCount.cshtml", new LinenCount(8));
}

public sealed record LinenCount(int? Towels);

public static class LinenCounts
{
    public static readonly AppEvent AddPack = AppEvent.Declare();

    public static readonly AppEvent AddOne = AppEvent.Declare();
}
