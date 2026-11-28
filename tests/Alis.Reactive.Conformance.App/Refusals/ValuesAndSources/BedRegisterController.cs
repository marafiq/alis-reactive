using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class BedRegisterController : Controller
{
    [HttpGet(RefusalPages.BedRegister)]
    public IActionResult Register() => View("/Refusals/ValuesAndSources/BedRegister.cshtml", new BedRegister("North ward"));

    [HttpGet(BedRegisters.Beds)]
    public IActionResult Beds() => Json(new RegisteredBeds(
    [
        new Record { Name = "Bed 1", Day = new DateTime(2026, 11, 9) },
        new Record { Name = "Bed 2", Day = new DateTime(2026, 11, 10) },
    ]));
}

public sealed record BedRegister(string Ward);

public sealed record RegisteredBeds(IReadOnlyList<Record> Beds);

public static class BedRegisters
{
    public const string Beds = RefusalPages.Area + "values-and-sources/bed-register/beds";
}
