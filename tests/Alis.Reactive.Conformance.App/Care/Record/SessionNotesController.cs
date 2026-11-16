using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class SessionNotesController : Controller
{
    [HttpGet(CarePages.SessionNotes)]
    public IActionResult OfAda() => View("/Care/Record/SessionNotes.cshtml", new SessionNotes("Ada Hart"));

    [HttpGet(SessionNotesEndpoints.ChartNotes)]
    public IActionResult ChartNotes() => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.SessionSignedOut
        ? Challenge()
        : Json(new SessionNote("Ate well at lunch"));

    [HttpGet(SessionNotesEndpoints.SignIn)]
    public IActionResult SignIn() => View("/Care/Record/SessionSignIn.cshtml");
}

[ApiController]
public sealed class SessionNotesApiController : ControllerBase
{
    [HttpGet(SessionNotesEndpoints.Notes)]
    public IActionResult Notes() => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.SessionSignedOut
        ? Challenge()
        : Ok(new SessionNote("Ate well at lunch"));
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
