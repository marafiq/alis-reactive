using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class CarerDirectoryController : Controller
{
    [HttpGet(RefusalPages.CarersByName)]
    public IActionResult ByName() => View("/Refusals/ArrayLambdas/CarerDirectory.cshtml", new CarerDirectory(World.ComponentVersion));

    [HttpGet(CarerDirectoryEndpoints.Carers)]
    public IActionResult Carers() => Json(World.Carers.Select(name => new Record { Name = name }).ToList());

    private WorldData World => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;
}

public static class CarerDirectoryEndpoints
{
    public const string Carers = RefusalPages.Area + "array-lambdas/carers-by-name/carers";
}
