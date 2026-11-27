using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AlertPreferencesController : Controller
{
    private const string Preferences = "/Care/RecordContacts/AlertPreferences.cshtml";

    [HttpGet(CarePages.AlertPreferences)]
    public IActionResult Paused() => View(Preferences, new AlertPreferences());

    [HttpGet(CarePages.AlertPreferencesEnabled)]
    public IActionResult Enabled() => View(Preferences, new AlertPreferences(NightAlerts: true));

    [HttpGet(CarePages.AlertPreferencesWardManaged)]
    public IActionResult WardManaged() => View(Preferences, new AlertPreferences(WardManaged: true));

    [HttpGet(CarePages.AlertPreferencesReplacement)]
    public IActionResult Replacement() => View(Preferences, new AlertPreferences(NightAlerts: true, ReplacingSetting: true));

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
    bool RightToLeft = false);

public sealed class AlertDestination
{
    public bool AlertDesk { get; init; } = true;
}

public sealed record AlertSwitchPreview(bool NightAlerts);

public sealed record AlertDestinationReceipt(string SelectedDesk);
