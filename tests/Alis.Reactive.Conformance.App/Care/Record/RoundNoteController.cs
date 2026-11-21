using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoundNoteController : Controller
{
    [HttpGet(CarePages.RoundNote)]
    public IActionResult OfAResident() => View("/Care/Record/RoundNote.cshtml", new RoundNote("Ada Hart", "Day round"));
}

public sealed record RoundNote(string Resident, string Round);
