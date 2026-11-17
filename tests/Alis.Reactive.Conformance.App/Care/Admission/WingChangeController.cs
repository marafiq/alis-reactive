using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WingChangeController : Controller
{
    [HttpGet(CarePages.WingChange)]
    public IActionResult ForAResidentInLily() => View("/Care/Admission/WingChange.cshtml", new WingChange { Wing = "Lily" });

    [HttpGet(CarePages.WingChangeWithNoWing)]
    public IActionResult ForAResidentWithNoWing() => View("/Care/Admission/WingChange.cshtml", new WingChange());
}

public sealed class WingChange
{
    public string? Wing { get; set; }
}

public static class WingChanges
{
    public static readonly AppEvent ChangeWing = AppEvent.Declare();
}
