using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class WardController : Controller
{
    [HttpGet(RefusalPages.Ward)]
    public IActionResult Ward() => View("/Refusals/Schema/Ward.cshtml", new Ward("North"));
}

public sealed record Ward(string Name);
