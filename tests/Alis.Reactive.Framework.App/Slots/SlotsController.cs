using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Slots;

public sealed class SlotsController : Controller
{
    public IActionResult Desk() => View(new DeskPage());

    public IActionResult Lounge() => View(new LoungePage());

    public IActionResult Hall() => View(new HallPage());

    public IActionResult PassDesk(PassDeskStep step) => View(new PassDeskPage { Step = step });

    public IActionResult Board() => PartialView("Board", new HallBoard());

    public IActionResult Note() => PartialView("Note", new HallNote());

    public IActionResult Plain() => PartialView("Plain");

    public IActionResult Nothing() => NoContent();

    public IActionResult VisitorCard() => PartialView("VisitorCard", new VisitorCard());

    public IActionResult Card()
    {
        ViewData.TemplateInfo.HtmlFieldPrefix = nameof(DeskPage.NextOfKin);
        return PartialView("Card", new KinCard());
    }

    [HttpPost]
    public IActionResult Kin([FromBody] DeskPage desk, [FromServices] DeskRules rules)
    {
        var result = rules.Validate(desk);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new KinEcho { Summary = $"next of kin {desk.NextOfKin.Name ?? "none"}" });
    }
}
