using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class ShiftRotaController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(RefusalPages.ShiftNoValueNames)]
    public IActionResult NoValueNames() => View("/Refusals/RequestsAndResponses/ShiftRota.cshtml", new ShiftRota("North", 5));

    [HttpGet(RefusalPages.ShiftAValueNames)]
    public IActionResult AValueNames() => View("/Refusals/RequestsAndResponses/ShiftRota.cshtml", new ShiftRota("North", 1));

    [HttpGet(RefusalEndpoints.Rota)]
    public IActionResult Rota(RotaOfShift query) => Content($"Rota of the {query.Shift} shift", PlainText);
}

public sealed record ShiftRota(string Ward, int ShiftOfTheDay);

public sealed class RotaOfShift
{
    public Shift Shift { get; set; }
}

public enum Shift
{
    Day,
    Night,
    Twilight,
}
