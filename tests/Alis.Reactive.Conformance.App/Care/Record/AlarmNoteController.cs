using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AlarmNoteController : Controller
{
    [HttpGet(CarePages.AlarmNote)]
    public IActionResult OfAResident() => View("/Care/Record/AlarmNote.cshtml", new AlarmNote("Ada Hart", "Morning round"));
}

public sealed record AlarmNote(string Resident, string Round);
