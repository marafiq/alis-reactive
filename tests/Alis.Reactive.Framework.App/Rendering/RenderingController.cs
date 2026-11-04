using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Rendering;

public sealed class RenderingController : Controller
{
    public IActionResult Handover() => View(new ShiftHandover());

    public IActionResult NotesInASection() => View(new ShiftHandover());

    public IActionResult SaveInASection() => View(new ShiftHandover());

    public IActionResult RenderedTwice() => View(new ShiftHandover());

    public IActionResult Rounds(VitalsCardPlan card) => View(new RoundsPage { Card = card });

    [HttpPost("/Rendering/Handovers")]
    public IActionResult Handovers([FromBody] ShiftHandover handover) => Json(new HandoverEcho
    {
        Summary = $"resident {handover.Resident ?? "none"}, notes {handover.Notes ?? "none"}",
    });

    public IActionResult Relief(ReliefPrefix prefix) => View(new ReliefNurse { Prefix = prefix });

    [HttpPost("/Rendering/Reliefs")]
    public IActionResult Reliefs([FromBody] ReliefShift shift) => Json(new HandoverEcho
    {
        Summary = $"relief {shift.Relief.Name ?? "none"}",
    });
}
