using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.EJ2.PivotView;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardCensusController : Controller
{
    [HttpGet(CarePages.WardCensus)]
    public IActionResult AsAChart() => View("/Care/BedBoard/WardCensus.cshtml", new WardCensus(Syncfusion.EJ2.PivotView.View.Chart, Residents()));

    [HttpGet(CarePages.WardCensusAsTable)]
    public IActionResult AsATable() => View("/Care/BedBoard/WardCensus.cshtml", new WardCensus(Syncfusion.EJ2.PivotView.View.Table, Residents()));

    private static IReadOnlyList<CensusEntry> Residents() => [new CensusEntry("North", 1), new CensusEntry("North", 1), new CensusEntry("East", 1)];
}

public sealed record WardCensus(View ShownAs, IReadOnlyList<CensusEntry> Residents);

public sealed record CensusEntry(string Ward, int Residents);

public static class CensusViews
{
    public static readonly AppEvent WhichView = AppEvent.Declare();
}
