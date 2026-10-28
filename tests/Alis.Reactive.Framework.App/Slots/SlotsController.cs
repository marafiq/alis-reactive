using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Slots;

/// <summary>A partial loaded into a slot and unloaded from it (capabilities L, E6, I4, B1).</summary>
public sealed class SlotsController : Controller
{
    public IActionResult Desk() => View(new DeskPage());

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
