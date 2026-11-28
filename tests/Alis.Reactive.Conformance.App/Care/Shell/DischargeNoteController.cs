using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DischargeNoteController : Controller
{
    [HttpGet(CarePages.DischargeNote)]
    public IActionResult OfAResident() => View("/Care/Shell/DischargeNote.cshtml", new DischargeNote());
}

public sealed class DischargeNote
{
    public string? Note { get; set; }
}

public static class DischargeNoteButtons
{
    public static readonly AppEvent Discharge = AppEvent.Declare();
}
