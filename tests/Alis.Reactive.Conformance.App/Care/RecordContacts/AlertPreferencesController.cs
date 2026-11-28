using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AlertPreferencesController : Controller
{
    private const string Preferences = "/Care/RecordContacts/AlertPreferences.cshtml";

    private AlertPreferenceBook Book => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.Owns(AlertPreferenceBook.Open);

    [HttpGet(CarePages.AlertPreferences)]
    public IActionResult Paused(bool reviewChannel = false, bool explicitChannelLabel = true)
        => View(Preferences, new AlertPreferences(NightAlerts: Book.NightAlerts, ReviewChannel: reviewChannel, ExplicitChannelLabel: explicitChannelLabel));

    [HttpPost(CarePages.AlertPreferencesSave)]
    public IActionResult Save([FromForm] AlertPreferenceChange preference)
    {
        if (preference.NightAlerts is not { } enabled) return BadRequest();

        Book.Save(enabled);
        return Content(Book.NightAlerts ? "Night alerts saved as enabled" : "Night alerts saved as paused", "text/plain");
    }

    [HttpGet(CarePages.AlertPreferencesEnabled)]
    public IActionResult Enabled(bool reviewChannel = false) => View(Preferences, new AlertPreferences(NightAlerts: true, ReviewChannel: reviewChannel));

    [HttpGet(CarePages.AlertPreferencesWardManaged)]
    public IActionResult WardManaged() => View(Preferences, new AlertPreferences(WardManaged: true));

    [HttpGet(CarePages.AlertPreferencesReplacement)]
    public IActionResult Replacement() => View(Preferences, new AlertPreferences(NightAlerts: true, ReplacingSetting: true));

    [HttpGet(CarePages.AlertPreferencesReplacementPaused)]
    public IActionResult ReplacementPaused() => View(Preferences, new AlertPreferences(ReplacingSetting: true));

    [HttpGet(CarePages.AlertPreferencesBrowserStorage)]
    public IActionResult BrowserStorage() => View(Preferences, new AlertPreferences(ReviewStorage: true));

    [HttpGet(CarePages.AlertPreferencesSavedOnBrowser)]
    public IActionResult SavedOnBrowser() => View(Preferences, new AlertPreferences(ReviewStorage: true, StartWithPersistence: true));

    [HttpGet(CarePages.AlertPreferencesLocked)]
    public IActionResult Locked() => View(Preferences, new AlertPreferences(Locked: true));

    [HttpGet(CarePages.AlertPreferencesCompact)]
    public IActionResult Compact() => View(Preferences, new AlertPreferences(Compact: true));

    [HttpGet(CarePages.AlertPreferencesRightToLeft)]
    public IActionResult RightToLeft() => View(Preferences, new AlertPreferences(RightToLeft: true));

    [HttpGet(CarePages.AlertSwitchPreview)]
    public IActionResult Preview() => View("/Care/RecordContacts/AlertSwitchPreview.cshtml", new AlertSwitchPreview(NightAlerts: true));

    [HttpGet(CarePages.AlertDestination)]
    public IActionResult Destination() => View("/Care/RecordContacts/AlertDestination.cshtml", new AlertDestination());

    [HttpPost(CarePages.AlertDestination)]
    public IActionResult SaveDestination([FromForm] string? alertDesk) => View("/Care/RecordContacts/AlertDestinationSaved.cshtml",
        new AlertDestinationReceipt(alertDesk switch
        {
            "night" => "Night desk selected",
            "day" => "Day desk selected",
            _ => "No desk selected"
        }));
}

public sealed record AlertPreferences(
    bool NightAlerts = false,
    bool WardManaged = false,
    bool ReplacingSetting = false,
    bool Locked = false,
    bool Compact = false,
    bool RightToLeft = false,
    bool ReviewChannel = false,
    bool ExplicitChannelLabel = true,
    bool ReviewStorage = false,
    bool StartWithPersistence = false);

public sealed record AlertPreferenceChange(bool? NightAlerts);

internal sealed class AlertPreferenceBook : IOwnedByAWorld
{
    private volatile bool _nightAlerts;

    internal bool NightAlerts => _nightAlerts;

    internal static AlertPreferenceBook Open() => new();

    internal void Save(bool nightAlerts) => _nightAlerts = nightAlerts;

    void IOwnedByAWorld.Take(Seed seed) { }
}

public sealed class AlertDestination
{
    public bool AlertDesk { get; init; } = true;
}

public sealed record AlertSwitchPreview(bool NightAlerts);

public sealed record AlertDestinationReceipt(string SelectedDesk);
