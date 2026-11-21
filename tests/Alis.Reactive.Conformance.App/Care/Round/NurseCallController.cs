using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class NurseCallController : Controller
{
    [HttpGet(CarePages.NurseCall)]
    public IActionResult HeardFirst() => View("/Care/Round/NurseCall.cshtml", new NurseCall("Room 4"));

    [HttpGet(CarePages.NurseCallCopiedFirst)]
    public IActionResult CopiedFirst() => View("/Care/Round/NurseCallCopiedFirst.cshtml", new NurseCall("Room 4"));
}

public sealed record NurseCall(string Room);

public static class NurseCalls
{
    public static readonly AppEvent Call = AppEvent.Declare();
}
