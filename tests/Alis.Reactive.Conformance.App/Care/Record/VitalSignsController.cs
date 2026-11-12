using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class VitalSignsController : Controller
{
    [HttpGet(CarePages.VitalSigns)]
    public IActionResult OfAResident() => View("/Care/Record/VitalSigns.cshtml", new VitalSigns("Ada Hart"));
}

public sealed record VitalSigns(string Resident);

public sealed class PulseReading
{
    public int? Pulse { get; init; }
}

public static class VitalSignsEvents
{
    public static readonly AppEvent NoPulse = AppEvent.Declare();

    public static readonly AppEvent PulseOfSeventy = AppEvent.Declare();

    public static readonly AppEvent<PulseReading> ReadingTaken = AppEvent.Declare<PulseReading>();
}
