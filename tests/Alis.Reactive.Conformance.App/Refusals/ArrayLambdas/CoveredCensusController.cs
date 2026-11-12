using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class CoveredCensusController : Controller
{
    [HttpGet(RefusalPages.CensusWithNoCover)]
    public IActionResult WithNoCover() => View("/Refusals/ArrayLambdas/CoveredCensus.cshtml", new CoveredCensus(null!));

    [HttpGet(RefusalPages.CensusWithACover)]
    public IActionResult WithACover() => View("/Refusals/ArrayLambdas/CoveredCensus.cshtml", new CoveredCensus(new CoverCarer("Ada")));

    [HttpGet(ArrayLambdaEndpoints.CensusResidents)]
    public IActionResult Residents() => Json(new List<CensusResident>
    {
        new("Ben Ode", "Ada"),
        new("Cy Roe", "Eve"),
        new("Di Lam", "Ada"),
    });
}

public sealed record CoveredCensus(CoverCarer Cover);

public sealed record CoverCarer(string Name);

public sealed record CensusResident(string Name, string Lead);
