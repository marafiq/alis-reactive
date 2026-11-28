using System.Text.Json;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WristbandNamesController : Controller
{
    private const string Editor = "/Care/RecordContacts/WristbandNames.cshtml";

    private WristbandFile FileOfResident => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.Owns(WristbandFile.Open);

    [HttpGet(CarePages.WristbandNames)]
    public IActionResult Names() => View(Editor, new WristbandNames("Nell"));

    [HttpGet(CarePages.WristbandNameBlank)]
    public IActionResult Unnamed() => View(Editor, new WristbandNames(null));

    [HttpGet(CarePages.WristbandNameLocked)]
    public IActionResult Locked() => View(Editor, new WristbandNames(null, Locked: true));

    [HttpGet(CarePages.WristbandNameProtected)]
    public IActionResult Protected() => View(Editor, new WristbandNames(null, Protected: true));

    [HttpGet(CarePages.WristbandNameRemembered)]
    public IActionResult Remembered() => View(Editor, new WristbandNames("Nell", Remember: true));

    [HttpGet(CarePages.WristbandNameSaved)]
    public IActionResult Saved() => View(Editor, new WristbandNames(FileOfResident.PreferredName));

    [HttpPost(CarePages.WristbandNameSave)]
    public IActionResult Save([FromForm] WristbandNameChange change)
    {
        FileOfResident.Save(change.PreferredName);
        return Content(FileOfResident.PreferredName is null ? "Unnamed profile saved" : "Named profile saved");
    }

    [HttpPost(CarePages.WristbandNameRecovery)]
    public IActionResult Recover([FromBody] WristbandArchive archive)
    {
        var snapshot = JsonSerializer.Deserialize<WristbandSnapshot>(archive.Snapshot, JsonSerializerOptions.Web);
        FileOfResident.Save(snapshot?.Value);
        return Content(FileOfResident.PreferredName is { } name ? "Recovery copy for " + name : "No name in recovery copy");
    }
}

public sealed record WristbandNames(string? PreferredName, bool Locked = false, bool Protected = false, bool Remember = false);

public sealed record WristbandNameChange(string? PreferredName);

public sealed record WristbandArchive(string Snapshot);

public sealed record WristbandSnapshot(string? Value);

internal sealed class WristbandFile : IOwnedByAWorld
{
    private volatile string? _preferredName = "Nell";

    internal string? PreferredName => _preferredName;

    internal static WristbandFile Open() => new();

    internal void Save(string? name) => _preferredName = string.IsNullOrEmpty(name) ? null : name;

    void IOwnedByAWorld.Take(Seed seed) { }
}
