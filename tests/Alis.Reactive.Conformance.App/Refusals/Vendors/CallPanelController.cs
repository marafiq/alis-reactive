using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class CallPanelController : Controller
{
    [HttpGet(RefusalPages.CallPanelOfTwoDeclarations)]
    public IActionResult TwoDeclarations() => View("/Refusals/Vendors/CallPanelOfTwoDeclarations.cshtml", new CallPanel("North"));

    [HttpGet(RefusalPages.CallPanelOfOneDeclaration)]
    public IActionResult OneDeclaration() => View("/Refusals/Vendors/CallPanelOfOneDeclaration.cshtml", new CallPanel("North"));
}

public sealed record CallPanel(string Ward);

public static class CallPanels
{
    public static readonly AppEvent PingAndWrite = AppEvent.Declare();
}
