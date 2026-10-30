using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Slots;

/// <summary>
/// The move-in, a wizard whose first step the server renders with the page inside the container the steps
/// load into (grammar 3.5). Routed under the Slots slice, its views live in the slice's folder.
/// </summary>
[Route("Slots/MoveIn")]
public sealed class MoveInController : Controller
{
    [HttpGet("")]
    public IActionResult Index() => View("/Slots/MoveIn.cshtml", new MoveInPage { Coordinator = "Dana" });

    /// <summary>Checks the resident step as the step checks itself, then answers with the room step.</summary>
    [HttpPost("Resident")]
    public IActionResult Resident([FromBody] MoveInResident resident, [FromServices] MoveInResidentRules rules)
    {
        var result = rules.Validate(resident);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return PartialView("/Slots/MoveInRoom.cshtml", new MoveInRoom());
    }

    /// <summary>Keeps the move-in for later by the page's rules, and says who took it.</summary>
    [HttpPost("Save")]
    public IActionResult Save([FromBody] MoveInPage moveIn, [FromServices] MoveInRules rules)
    {
        var result = rules.Validate(moveIn);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new MoveInSaved { Summary = $"the move-in taken by {moveIn.Coordinator} is saved for later" });
    }
}
