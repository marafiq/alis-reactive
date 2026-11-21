using System.Globalization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class StrengthIntakeController : Controller
{
    private const string Html = "text/html";

    [HttpGet(CarePages.StrengthIntake)]
    public IActionResult OfAResident() => View("/Care/Admission/StrengthIntake.cshtml", new StrengthIntake());

    [HttpPost(StrengthIntakeEndpoints.Intakes)]
    public IActionResult Save([FromForm] StrengthIntake intake)
        => Content($"<p>{intake.Strength.ToString(CultureInfo.InvariantCulture)}</p>", Html);
}

public sealed class StrengthIntake
{
    public decimal Strength { get; set; }
}

public static class StrengthIntakeEndpoints
{
    public const string Intakes = CarePages.StrengthIntake + "/intakes";

    public const string IntakesInGerman = Intakes + "?culture=de-DE";
}
