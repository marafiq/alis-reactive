using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BedCommitController : Controller
{
    [HttpGet(CarePages.BedCommit)]
    public IActionResult WithACatchAll() => View("/Care/Admission/BedCommit.cshtml", new BedAssignment("Ada Hart", "E4"));

    [HttpGet(CarePages.BedCommitWithAConflictRoute)]
    public IActionResult WithAConflictRoute() => View("/Care/Admission/BedCommitWithAConflictRoute.cshtml", new BedAssignment("Ada Hart", "E4"));

    [HttpPost(BedCommitEndpoints.Beds)]
    public IActionResult Commit()
    {
        if (!HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.BedsDropTheirConnection) return Conflict(new CommitError("Bed taken"));

        HttpContext.Abort();
        return new EmptyResult();
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
