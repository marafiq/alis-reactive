using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class NotesRefreshController : Controller
{
    private const string PlainText = "text/plain";

    private const string RequestedWith = "X-Requested-With";

    [HttpGet(CarePages.NotesRefresh)]
    public IActionResult OfAda() => View("/Care/Record/NotesRefresh.cshtml", new NotesRefresh("Ada Hart"));

    [HttpGet(NotesRefreshEndpoints.Notes)]
    public IActionResult Notes() => Content(string.Join(", ", Request.Headers[RequestedWith].ToArray()), PlainText);
}

public sealed record NotesRefresh(string Resident);

public static class NotesRefreshEndpoints
{
    public const string Notes = CarePages.NotesRefresh + "/notes";
}

public static class NotesRefreshes
{
    public static readonly AppEvent Refresh = AppEvent.Declare();

    public static readonly AppEvent RefreshByFetch = AppEvent.Declare();
}
