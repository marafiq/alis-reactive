using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Partials;

/// <summary>A page and its partials composing by the member an input binds: an editor template, a partial of a base class.</summary>
public sealed class PartialsController : Controller
{
    public IActionResult Desk() => View(new DeskPage());

    /// <summary>Binds the desk as any endpoint does and checks it by the desk's rules; then says whom it bound.</summary>
    [HttpPost]
    public IActionResult SaveDesk([FromBody] DeskPage desk, [FromServices] DeskRules rules)
    {
        var result = rules.Validate(desk);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Said($"visitor {desk.Visitor ?? "none"}, next of kin {desk.NextOfKin.Name ?? "none"}");
    }

    /// <summary>Checks the next of kin against the resident's file, so a message names the member as the desk binds it (NextOfKin.Name).</summary>
    [HttpPost]
    public IActionResult CheckKin([FromBody] DeskPage desk, [FromServices] KinOnFileRules rules)
    {
        var result = rules.Validate(desk);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Said($"next of kin {desk.NextOfKin.Name} is on file");
    }

    public IActionResult Lobby() => View(new LobbyPage());

    private JsonResult Said(string summary) => Json(new PartialsEcho { Summary = summary });
}
