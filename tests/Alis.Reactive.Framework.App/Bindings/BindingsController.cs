using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Bindings;

/// <summary>
/// The names a pipeline binds and reads again (grammar.md 3.1): an awaited method's result read by a
/// request's parts, by the steps after a check, a chained request and a branch, and by requests started
/// together, each where the plan reads it.
/// </summary>
public sealed class BindingsController : Controller
{
    public IActionResult Round() => View(new RoundPage());

    /// <summary>Says what the desk was told, in its own words.</summary>
    [HttpPost("/Bindings/Findings")]
    public IActionResult Finding([FromBody] Finding finding) =>
        Json(new Answer { Summary = $"{finding.Desk ?? "the desk"} was told {finding.Found ?? "nothing"}" });

    [HttpGet("/Bindings/Opened")]
    public IActionResult Opened() => Json(new Answer { Summary = "the round is open" });
}
