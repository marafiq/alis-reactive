using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class SessionNotesController : Controller
{
    private WorldData World => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;

    [HttpGet(CarePages.SessionNotes)]
    public IActionResult OfAda() => View("/Care/Record/SessionNotes.cshtml", new SessionNotes("Ada Hart"));

    [HttpGet(SessionNotesEndpoints.ChartNotes)]
    public IActionResult ChartNotes() => World.SessionSignedOut
        ? Challenge()
        : Json(World.Owns(ChartBook.Open).LatestNote);

    [HttpGet(SessionNotesEndpoints.SignIn)]
    public IActionResult SignIn() => View("/Care/Record/SessionSignIn.cshtml");
}

[ApiController]
public sealed class SessionNotesApiController : ControllerBase
{
    private WorldData World => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;

    [HttpGet(SessionNotesEndpoints.Notes)]
    public IActionResult Notes() => World.SessionSignedOut
        ? Challenge()
        : Ok(World.Owns(ChartBook.Open).LatestNote);
}

public sealed record SessionNotes(string Resident);

public sealed record SessionNote(string Text);

public static class SessionNotesEndpoints
{
    public const string Notes = CarePages.SessionNotes + "/notes";

    public const string ChartNotes = CarePages.SessionNotes + "/chart";

    public const string SignIn = CarePages.SessionNotes + "/sign-in";
}

public static class SessionNotesEvents
{
    public static readonly AppEvent Refresh = AppEvent.Declare();

    public static readonly AppEvent RefreshFromTheChart = AppEvent.Declare();
}

internal sealed class ChartBook : IOwnedByAWorld
{
    private readonly string _latestNote = "Ate well at lunch";

    internal static ChartBook Open() => new();

    void IOwnedByAWorld.Take(Seed seed)
    {
    }

    internal SessionNote LatestNote => new(_latestNote);
}
