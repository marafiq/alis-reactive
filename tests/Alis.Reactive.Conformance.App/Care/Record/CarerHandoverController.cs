using System.Collections.Concurrent;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CarerHandoverController : Controller
{
    private const string NoCarerOnCall = "no carer on call";

    private HandoverBook Book => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.Owns(HandoverBook.Open);

    [HttpGet(CarePages.CarerHandover)]
    public IActionResult OnTheRecord() => View("/Care/Record/CarerHandover.cshtml", new CarerHandover("Ada Hart", "Ada"));

    [HttpGet(CarerHandoverEndpoints.NightRoster)]
    public IActionResult Roster() => Json(Book.OnCall is { } onCall ? new NightRoster(onCall) : null);

    [HttpPost(CarerHandoverEndpoints.Handovers)]
    public IActionResult HandOver([FromBody] CarersHandedOver? handedOver)
    {
        if (handedOver is null || handedOver.Carers.Count == 0) return BadRequest();

        var recorded = Book.Record(handedOver.Carers);
        return Content($"Handover recorded: {string.Join(", ", recorded.Select(carer => carer ?? NoCarerOnCall))}", "text/plain");
    }
}

internal sealed class HandoverBook : IOwnedByAWorld
{
    private readonly ConcurrentQueue<string> _nightRoster = new();
    private readonly ConcurrentQueue<IReadOnlyList<string?>> _handovers = new();

    internal static HandoverBook Open() => new();

    internal string? OnCall => _nightRoster.TryPeek(out var carer) ? carer : null;

    void IOwnedByAWorld.Take(Seed seed)
    {
        foreach (var carer in seed.Carers) _nightRoster.Enqueue(carer);
    }

    internal IReadOnlyList<string?> Record(IReadOnlyList<string?> carers)
    {
        var handover = carers.ToArray();
        _handovers.Enqueue(handover);
        return handover;
    }
}

public sealed record CarerHandover(string Resident, string DayCarer);

public sealed record NightRoster(string OnCall);

public sealed class CarersHandedOver
{
    public IReadOnlyList<string?> Carers { get; set; } = [];
}

public static class CarerHandoverEndpoints
{
    public const string NightRoster = CarePages.CarerHandover + "/night-roster";
    public const string Handovers = CarePages.CarerHandover + "/handovers";
}

public static class CarerHandovers
{
    public static readonly AppEvent Send = AppEvent.Declare();
    public static readonly AppEvent SendDay = AppEvent.Declare();
}
