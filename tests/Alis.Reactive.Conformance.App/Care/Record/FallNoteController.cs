using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class FallNoteController : Controller
{
    [HttpGet(CarePages.FallNote)]
    public IActionResult OfAResident() => View("/Care/Record/FallNote.cshtml", new FallNote("Ada Hart", "Night round"));
}

public sealed record FallNote(string Resident, string Round);
