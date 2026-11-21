using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BedAllocationController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.BedAllocation)]
    public IActionResult OfAResident()
        => View("/Care/AdmissionClinical/BedAllocation.cshtml", new BedAllocation { Beds = [new CareBed("12A Maple", "12A"), new CareBed("14B Cedar", "14B")] });

    [HttpPost(BedAllocationEndpoints.Allocations)]
    public IActionResult Save([FromForm] BedAllocation allocation) => Content($"Bed: {allocation.BedCode}", PlainText);
}

public sealed class BedAllocation
{
    public string? BedCode { get; set; }

    public IReadOnlyList<CareBed> Beds { get; init; } = [];
}

public sealed record CareBed([property: JsonPropertyName("label")] string DisplayLabel, string BedCode);

public static class BedAllocationEndpoints
{
    public const string Allocations = CarePages.BedAllocation + "/allocations";
}

public static class BedAllocations
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
