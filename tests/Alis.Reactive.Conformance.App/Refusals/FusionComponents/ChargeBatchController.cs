using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class ChargeBatchController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(RefusalPages.ChargeBatch)]
    public IActionResult OfTheWing() => View("/Refusals/FusionComponents/ChargeBatch.cshtml", new ChargeBatch
    {
        Charges = [new BatchCharge { Name = "Ada", DueOn = new DateTime(2026, 11, 10), Status = BatchChargeStatus.Due }],
    });

    [HttpPost(ChargeBatchEvents.Saved)]
    public IActionResult Saved([FromBody] SavedBatch saved) => Content(saved.Changed.Any() ? "Saved " + string.Join(", ", saved.Changed.Select(charge => charge.Name)) : "No charges changed", PlainText);
}

public sealed class ChargeBatch
{
    public IReadOnlyList<BatchCharge> Charges { get; init; } = [];
}

public sealed class SavedBatch
{
    public IEnumerable<BatchCharge> Changed { get; init; } = [];
}

public sealed class BatchCharge
{
    public string Name { get; init; } = "";

    public DateTime DueOn { get; init; }

    public BatchChargeStatus Status { get; init; }
}

public enum BatchChargeStatus
{
    Due,

    Paid,
}

public static class ChargeBatchEvents
{
    public const string Saved = RefusalPages.ChargeBatch + "/saved";

    public static readonly AppEvent Save = AppEvent.Declare();
}
