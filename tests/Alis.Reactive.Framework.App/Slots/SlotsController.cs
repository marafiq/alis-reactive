using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Slots;

/// <summary>A partial loaded into a slot and unloaded from it (capabilities L, E6, I4, B1).</summary>
public sealed class SlotsController : Controller
{
    public IActionResult Desk() => View(new DeskPage());

    public IActionResult Lounge() => View(new LoungePage());

    public IActionResult Hall() => View(new HallPage());

    /// <summary>The pass desk, with the one step it writes about its slot.</summary>
    public IActionResult PassDesk(PassDeskStep step) => View(new PassDeskPage { Step = step });

    /// <summary>The notice board, loaded into the hall's slot; it holds a slot of its own.</summary>
    public IActionResult Board() => PartialView("Board", new HallBoard());

    /// <summary>A note pinned into the board's slot.</summary>
    public IActionResult Note() => PartialView("Note", new HallNote());

    /// <summary>A notice with no plan.</summary>
    public IActionResult Plain() => PartialView("Plain");

    /// <summary>An answer with nothing in it.</summary>
    public IActionResult Nothing() => NoContent();

    /// <summary>The visitor card, loaded into the lounge's slot.</summary>
    public IActionResult VisitorCard() => PartialView("VisitorCard", new VisitorCard());

    /// <summary>The next-of-kin card, rendered for the desk's NextOfKin member, so its inputs bind under that prefix.</summary>
    public IActionResult Card()
    {
        ViewData.TemplateInfo.HtmlFieldPrefix = nameof(DeskPage.NextOfKin);
        return PartialView("Card", new KinCard());
    }

    /// <summary>
    /// Binds the desk as any endpoint does and checks it by the desk's rules, so a message names the member
    /// as the desk binds it (NextOfKin.Name); then says whom it bound as next of kin.
    /// </summary>
    [HttpPost]
    public IActionResult Kin([FromBody] DeskPage desk, [FromServices] DeskRules rules)
    {
        var result = rules.Validate(desk);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new KinEcho { Summary = $"next of kin {desk.NextOfKin.Name ?? "none"}" });
    }
}
