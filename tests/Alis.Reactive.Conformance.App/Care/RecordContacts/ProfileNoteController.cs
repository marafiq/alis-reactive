using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ProfileNoteController : Controller
{
    private const string Profile = "/Care/RecordContacts/ProfileNote.cshtml";

    [HttpGet(CarePages.ProfileNoteWritten)]
    public IActionResult Written() => View(Profile, new ProfileNote { CareNote = "Prefers tea at seven" });

    [HttpGet(CarePages.ProfileNoteBlank)]
    public IActionResult Blank() => View(Profile, new ProfileNote());
}

public sealed class ProfileNote
{
    public string? CareNote { get; set; }
}
