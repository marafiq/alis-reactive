using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Bindings;

public sealed class BindingsController : Controller
{
    public IActionResult Round() => View(new RoundPage());

    [HttpPost("/Bindings/Findings")]
    public IActionResult Finding([FromBody] Finding finding) =>
        Json(new Answer { Summary = $"{finding.Desk ?? "the desk"} was told {finding.Found ?? "nothing"}" });

    [HttpGet("/Bindings/Opened")]
    public IActionResult Opened() => Json(new Answer { Summary = "the round is open" });
}
