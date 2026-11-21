using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CarersOnDutyController : Controller
{
    [HttpGet(CarePages.CarersOnDuty)]
    public IActionResult OfTheWing() => View("/Care/Residents/CarersOnDuty.cshtml", new CarersOnDuty("North wing"));

    [HttpGet(CarersOnDutyEndpoints.Carers)]
    public IActionResult Carers() => Json(HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.Carers.Select(name => new DutyCarer { Name = name }).ToList());
}

public sealed record CarersOnDuty(string Wing);

public sealed class DutyCarer
{
    public string Name { get; set; } = "";
}

public static class CarersOnDutyEndpoints
{
    public const string Carers = CarePages.CarersOnDuty + "/carers";
}
