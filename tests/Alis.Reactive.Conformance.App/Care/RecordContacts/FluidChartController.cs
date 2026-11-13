using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class FluidChartController : Controller
{
    private const string Chart = "/Care/RecordContacts/FluidChart.cshtml";

    [HttpGet(CarePages.FluidChartOfIntake)]
    public IActionResult OfIntake() => View(Chart, new FluidChart { Title = "Fluid intake", Intake = [new FluidIntake(1200, 1500)] });

    [HttpGet(CarePages.FluidChartOfWeight)]
    public IActionResult OfWeight() => View(Chart, new FluidChart { Title = "Weight", Intake = [new FluidIntake(1200, 1500)] });
}

public sealed class FluidChart
{
    public required string Title { get; init; }

    public IReadOnlyList<FluidIntake> Intake { get; init; } = [];
}

public sealed record FluidIntake(int Taken, int Target);
