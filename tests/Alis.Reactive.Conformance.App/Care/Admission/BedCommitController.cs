using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BedCommitController : Controller
{
    private WorldData World => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;

    [HttpGet(CarePages.BedCommit)]
    public IActionResult WithACatchAll() => View("/Care/Admission/BedCommit.cshtml", new BedAssignment("Ada Hart", "E4"));

    [HttpGet(CarePages.BedCommitWithAConflictRoute)]
    public IActionResult WithAConflictRoute() => View("/Care/Admission/BedCommitWithAConflictRoute.cshtml", new BedAssignment("Ada Hart", "E4"));

    [HttpPost(BedCommitEndpoints.Beds)]
    public IActionResult Commit([FromForm] BedAssignment assignment)
    {
        if (World.BedsDropTheirConnection)
        {
            HttpContext.Abort();
            return new EmptyResult();
        }

        var taken = World.Owns(BedBook.Open).Assign(assignment);
        return taken is null ? NoContent() : Conflict(taken);
    }
}

public sealed record BedAssignment(string Resident, string Bed);

public sealed record CommitError(string Message);

public static class BedCommitEndpoints
{
    public const string Beds = "/Beds";
}

public static class BedCommits
{
    public static readonly AppEvent Save = AppEvent.Declare();
}

internal sealed class BedBook : IOwnedByAWorld
{
    private readonly object _gate = new();
    private readonly Dictionary<string, string> _holders = new(StringComparer.Ordinal) { ["E4"] = "Ben Okafor" };

    internal static BedBook Open() => new();

    void IOwnedByAWorld.Take(Seed seed)
    {
    }

    internal CommitError? Assign(BedAssignment assignment)
    {
        lock (_gate)
        {
            var heldByAnother = _holders.TryGetValue(assignment.Bed, out var holder) && holder != assignment.Resident;
            if (heldByAnother) return new CommitError("Bed taken");

            _holders[assignment.Bed] = assignment.Resident;
            return null;
        }
    }
}
