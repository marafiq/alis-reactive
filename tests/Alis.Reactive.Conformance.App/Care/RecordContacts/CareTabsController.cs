using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CareTabsController : Controller
{
    private const string Tabs = "/Care/RecordContacts/CareTabs.cshtml";

    [HttpGet(CarePages.CareTabsAtMedications)]
    public IActionResult AtMedications() => View(Tabs, new CareTabs { OpenTab = 2 });

    [HttpGet(CarePages.CareTabsAtVisits)]
    public IActionResult AtVisits() => View(Tabs, new CareTabs { OpenTab = 1 });
}

public sealed class CareTabs
{
    public double OpenTab { get; init; }
}

public static class CareTabReads
{
    public static readonly AppEvent Which = AppEvent.Declare();
}
