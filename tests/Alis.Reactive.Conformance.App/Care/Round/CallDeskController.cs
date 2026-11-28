using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CallDeskController : Controller
{
    [HttpGet(CarePages.CallDesk)]
    public IActionResult WithItsLog() => View("/Care/Round/CallDesk.cshtml", new CallDesk("North"));

    [HttpGet(CarePages.CallDeskWithNoLogHandler)]
    public IActionResult WithNoLogHandler() => View("/Care/Round/CallDeskWithNoLogHandler.cshtml", new CallDesk("North"));
}

public sealed record CallDesk(string Ward);

public static class CallDesks
{
    public static readonly AppEvent LogTheCall = AppEvent.Declare();
    public static readonly AppEvent CallLogged = AppEvent.Declare();
}
