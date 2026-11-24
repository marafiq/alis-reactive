using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ShiftHandoverController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.ShiftHandover)]
    public IActionResult Handover() => View("/Care/Admission/ShiftHandover.cshtml", new HandoverNote());

    [HttpPost(ShiftHandoverEndpoints.Handovers)]
    public IActionResult HandOver([FromBody] HandoverNote handover) => Content($"Handed over to the {handover.Shift} shift", PlainText);
}

public enum HandoverShift
{
    Day,
    Night,
}

public sealed class HandoverNote
{
    public string? Note { get; set; }

    public HandoverShift Shift { get; set; }
}

public static class ShiftHandoverLists
{
    public static IReadOnlyList<SelectListItem> Shifts() => [new SelectListItem("Day", "Day"), new SelectListItem("Night", "Night")];
}

public static class ShiftHandoverEndpoints
{
    public const string Handovers = CarePages.ShiftHandover + "/handovers";
}
