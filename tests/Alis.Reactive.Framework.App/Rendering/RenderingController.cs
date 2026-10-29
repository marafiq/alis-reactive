using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Rendering;

/// <summary>Where a view renders its plan, and what a page is refused with when its plan cannot hold what the view wrote.</summary>
public sealed class RenderingController : Controller
{
    /// <summary>The handover, its plan rendered in the layout's last section.</summary>
    public IActionResult Handover() => View(new ShiftHandover());

    /// <summary>The handover, the notes written in a section after the plan rendered in the body.</summary>
    public IActionResult NotesInASection() => View(new ShiftHandover());

    /// <summary>The handover, its save written in a section after the plan rendered in the body.</summary>
    public IActionResult SaveInASection() => View(new ShiftHandover());

    /// <summary>The handover, its plan rendered in the body and again in the layout's last section.</summary>
    public IActionResult RenderedTwice() => View(new ShiftHandover());

    /// <summary>The round: the handover with the resident's vitals card, whose plan renders where the address says.</summary>
    public IActionResult Rounds(VitalsCardPlan card) => View(new RoundsPage { Card = card });

    /// <summary>Binds the handover as any endpoint does, and says what it bound.</summary>
    [HttpPost("/Rendering/Handovers")]
    public IActionResult Handovers([FromBody] ShiftHandover handover) => Json(new HandoverEcho
    {
        Summary = $"resident {handover.Resident ?? "none"}, notes {handover.Notes ?? "none"}",
    });

    /// <summary>The relief nurse, rendered as the shift's Relief member.</summary>
    public IActionResult Relief(ReliefPrefix prefix) => View(new ReliefNurse { Prefix = prefix });

    /// <summary>Binds the shift the relief nurse takes over, and says whom it bound.</summary>
    [HttpPost("/Rendering/Reliefs")]
    public IActionResult Reliefs([FromBody] ReliefShift shift) => Json(new HandoverEcho
    {
        Summary = $"relief {shift.Relief.Name ?? "none"}",
    });
}
