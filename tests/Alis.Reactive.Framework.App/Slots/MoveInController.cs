using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Slots;

[Route("Slots/MoveIn")]
public sealed class MoveInController : Controller
{
    [HttpGet("")]
    public IActionResult Index() => View("/Slots/MoveIn.cshtml", new MoveInPage { Coordinator = "Dana" });

    [HttpPost("Resident")]
    public IActionResult Resident([FromBody] MoveInResident resident, [FromServices] MoveInResidentRules rules)
    {
        var result = rules.Validate(resident);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return PartialView("/Slots/MoveInRoom.cshtml", new MoveInRoom());
    }

    [HttpPost("Save")]
    public IActionResult Save([FromBody] MoveInPage moveIn, [FromServices] MoveInRules rules)
    {
        var result = rules.Validate(moveIn);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new MoveInSaved { Summary = $"the move-in taken by {moveIn.Coordinator} is saved for later" });
    }
}
