using System.Globalization;
using System.Text.Json;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ArrivalCalendarsController : Controller
{
    [HttpGet(CarePages.ArrivalPlanning)]
    public IActionResult ArrivalPlanning() => View("/Care/AdmissionClinical/ArrivalPlanning.cshtml", new ArrivalPlanning());

    [HttpGet(CarePages.ArrivalFormats)]
    public IActionResult ArrivalFormats() => View("/Care/AdmissionClinical/ArrivalFormats.cshtml", new ArrivalFormats());

    [HttpGet(CarePages.ArrivalWithdrawal)]
    public IActionResult ArrivalWithdrawal() => View("/Care/AdmissionClinical/ArrivalWithdrawal.cshtml", new ArrivalWithdrawal());

    [HttpGet(CarePages.ArrivalPrompt)]
    public IActionResult ArrivalPrompt() => View("/Care/AdmissionClinical/ArrivalPrompt.cshtml", new ArrivalPrompt());

    [HttpGet(CarePages.ArrivalRota)]
    public IActionResult ArrivalRota() => View("/Care/AdmissionClinical/ArrivalRota.cshtml", new ArrivalRota());

    [HttpGet(CarePages.ArrivalMask)]
    public IActionResult ArrivalMask() => View("/Care/AdmissionClinical/ArrivalMask.cshtml", new ArrivalMask());

    [HttpGet(CarePages.ArrivalSheet)]
    public IActionResult ArrivalSheet() => View("/Care/AdmissionClinical/ArrivalSheet.cshtml", new ArrivalSheet());

    [HttpGet(CarePages.ArrivalDraft)]
    public IActionResult ArrivalDraft() => View("/Care/AdmissionClinical/ArrivalDraft.cshtml", new ArrivalDraft());

    [HttpGet(CarePages.ArrivalSlips)]
    public IActionResult ArrivalSlips() => View("/Care/AdmissionClinical/ArrivalSlips.cshtml", new ArrivalSlips());

    [HttpGet(CarePages.ArrivalHandover)]
    public IActionResult ArrivalHandover() => View("/Care/AdmissionClinical/ArrivalHandover.cshtml", new ArrivalHandover());

    [HttpGet(CarePages.ArrivalZone)]
    public IActionResult ArrivalZone() => View("/Care/AdmissionClinical/ArrivalZone.cshtml", new ArrivalZone());

    [HttpGet(CarePages.ArrivalKeys)]
    public IActionResult ArrivalKeys() => View("/Care/AdmissionClinical/ArrivalKeys.cshtml", new ArrivalKeys());

    [HttpGet(CarePages.ArrivalCalendarSystem)]
    public IActionResult ArrivalCalendarSystem() => View("/Care/AdmissionClinical/ArrivalCalendarSystem.cshtml", new ArrivalCalendarSystem());

    [HttpGet(CarePages.ArrivalPlanningHeld)]
    public IActionResult ArrivalPlanningHeld() => View("/Care/AdmissionClinical/ArrivalPlanning.cshtml", new ArrivalPlanning { Held = true });

    [HttpGet(CarePages.ArrivalDraftReturn)]
    public IActionResult ArrivalDraftReturn() => View("/Care/AdmissionClinical/ArrivalDraft.cshtml", new ArrivalDraft { Remembered = true });

    [HttpGet(CarePages.ArrivalHandoverHeld)]
    public IActionResult ArrivalHandoverHeld() => View("/Care/AdmissionClinical/ArrivalHandover.cshtml", new ArrivalHandover { Held = true });

    [HttpGet(CarePages.ArrivalSavedLedger)]
    public IActionResult ArrivalSavedLedger() => View("/Care/AdmissionClinical/ArrivalSavedEdition.cshtml", new ArrivalSavedEdition { Ledger = true });

    [HttpGet(CarePages.ArrivalSavedLetter)]
    public IActionResult ArrivalSavedLetter() => View("/Care/AdmissionClinical/ArrivalSavedEdition.cshtml", new ArrivalSavedEdition { Ledger = false });

    [HttpPost(CarePages.ArrivalArchive)]
    public IActionResult Archive([FromForm] ArrivalRecovery request)
    {
        using var data = JsonDocument.Parse(request.Recovery);
        var value = data.RootElement.GetProperty("value");
        return Content(value.ValueKind == JsonValueKind.String && value.TryGetDateTime(out var day)
            ? "Archived arrival " + day.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture)
            : "No scheduled date in the archive", "text/plain");
    }
}

public sealed class ArrivalRecovery
{
    public string Recovery { get; set; } = string.Empty;
}

public sealed class ArrivalPlanning
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public bool Held { get; init; }
}

public sealed class ArrivalFormats
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);
}

public sealed class ArrivalWithdrawal
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);
}

public sealed class ArrivalPrompt
{
    public DateTime? ArrivesOn { get; set; }
}

public sealed class ArrivalRota
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2021, 1, 8);
}

public sealed class ArrivalMask
{
    public DateTime? ArrivesOn { get; set; }
}

public sealed class ArrivalSheet
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);
}

public sealed class ArrivalDraft
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public bool Remembered { get; init; }
}

public sealed class ArrivalSlips
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public DateTime? ReturnsOn { get; set; } = new DateTime(2026, 7, 20);
}

public sealed class ArrivalHandover
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public bool Held { get; init; }
}

public sealed class ArrivalSavedEdition
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public bool Ledger { get; init; }
}

public sealed class ArrivalZone
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14, 2, 0, 0);
}

public sealed class ArrivalKeys
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);
}

public sealed class ArrivalCalendarSystem
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);
}
