using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DoseLookupController : Controller
{
    [HttpGet(CarePages.DoseLookupOfATraceDose)]
    public IActionResult OfATraceDose() => View("/Care/Round/DoseLookup.cshtml", new DoseLookup(DoseLookupEndpoints.TraceDose));

    [HttpGet(CarePages.DoseLookupOfAHalfGramDose)]
    public IActionResult OfAHalfGramDose() => View("/Care/Round/DoseLookup.cshtml", new DoseLookup(DoseLookupEndpoints.HalfGramDose));

    [HttpGet(DoseLookupEndpoints.TraceDose)]
    public IActionResult TraceDose() => Json(new DoseFound { Grams = 0.00005 });

    [HttpGet(DoseLookupEndpoints.HalfGramDose)]
    public IActionResult HalfGramDose() => Json(new DoseFound { Grams = 0.5 });
}

public sealed record DoseLookup(string Dose);

public sealed class DoseFound
{
    public double Grams { get; init; }
}

public static class DoseLookupEndpoints
{
    private const string Root = CarePages.Area + "round/dose-lookup/";
    public const string TraceDose = Root + "dose/trace";
    public const string HalfGramDose = Root + "dose/half-gram";
}

public static class DoseLookups
{
    public static readonly AppEvent FindTheDose = AppEvent.Declare();
}
