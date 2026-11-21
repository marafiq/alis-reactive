using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardBedsController : Controller
{
    [HttpGet(CarePages.WardBeds)]
    public IActionResult OfAWard(WardPage page) => View("/Care/Residents/WardBeds.cshtml", page);
}

public sealed class WardPage
{
    public int Beds { get; set; } = 2;
}
