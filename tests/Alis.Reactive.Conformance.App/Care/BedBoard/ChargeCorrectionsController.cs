using System.Globalization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ChargeCorrectionsController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.ChargeCorrections)]
    public IActionResult OfTheWing() => View("/Care/BedBoard/ChargeCorrections.cshtml", new ChargeCorrections
    {
        Charges =
        [
            new CorrectedCharge { ResidentId = "R-101", Name = "Ada", DueOn = new DateTime(2026, 11, 10), Amount = 12.5m, Status = ChargeStatus.Due },
            new CorrectedCharge { ResidentId = "R-102", Name = "Rose", DueOn = new DateTime(2026, 11, 11), Amount = 40m, Status = ChargeStatus.Paid },
        ],
    });

    [HttpPost(ChargeCorrectionEvents.Saved)]
    public IActionResult Saved([FromBody] SavedCharges saved)
    {
        var charges = saved.Changed.Select(charge => "Saved " + charge.Name + " due " + charge.DueOn.ToString("yyyy-MM-dd'T'HH:mm", CultureInfo.InvariantCulture)
            + " at " + charge.Amount.ToString(CultureInfo.InvariantCulture) + " " + charge.Status).ToList();
        return Content(charges.Count == 0 ? "No charges changed" : string.Join("; ", charges), PlainText);
    }
}

public sealed class ChargeCorrections
{
    public IReadOnlyList<CorrectedCharge> Charges { get; init; } = [];
}

public sealed class SavedCharges
{
    public IEnumerable<CorrectedCharge> Changed { get; init; } = [];
}

public sealed class CorrectedCharge
{
    public string ResidentId { get; init; } = "";

    public string Name { get; init; } = "";

    public DateTime DueOn { get; init; }

    public decimal Amount { get; init; }

    public ChargeStatus Status { get; init; }
}

public enum ChargeStatus
{
    Due,

    Paid,
}

public static class ChargeCorrectionEvents
{
    public const string Saved = CarePages.Area + "bed-board/charge-corrections/saved";

    public static readonly AppEvent Save = AppEvent.Declare();
}
