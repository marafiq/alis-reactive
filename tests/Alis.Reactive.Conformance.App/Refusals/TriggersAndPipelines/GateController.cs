using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class GateController : Controller
{
    [HttpGet(RefusalPages.GateOfAnEscortedVisitor)]
    public IActionResult EscortedVisitor() => View("/Refusals/TriggersAndPipelines/GateOfAnEscortedVisitor.cshtml", new Gate("North gate", "Ada Hart"));

    [HttpGet(RefusalPages.GateOfAVisitorsType)]
    public IActionResult VisitorsType() => View("/Refusals/TriggersAndPipelines/GateOfAVisitorsType.cshtml", new Gate("North gate", "Ada Hart"));
}

public sealed record Gate(string Name, string Visitor);

public sealed class EscortedVisitor
{
    public int PartySize { get; init; }

    public Escort Escort { get; init; } = new("");
}

public sealed record Escort(string Name);

public sealed class TypedVisitor
{
    public int PartySize { get; init; }

    public VisitorType Type { get; init; }
}

public enum VisitorType
{
    Family,

    Clinician,
}

public static class EscortedGates
{
    public static readonly AppEvent<EscortedVisitor> Announced = AppEvent.Declare<EscortedVisitor>();
}

public static class Gates
{
    public static readonly AppEvent<TypedVisitor> Announced = AppEvent.Declare<TypedVisitor>();
}
