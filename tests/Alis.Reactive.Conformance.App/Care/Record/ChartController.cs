using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ChartController : Controller
{
    [HttpGet(CarePages.Chart)]
    public IActionResult OfAResident(ChartPage page) => View("/Care/Record/Chart.cshtml", page);
}

public sealed class ChartPage
{
    public string Tab { get; set; } = "vitals";
}
