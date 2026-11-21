using System.Globalization;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ChartSaveController(IDistributedCache charts) : Controller
{
    private const string PlainText = "text/plain";
    private const string SavesOnFile = "admission-chart-saves";

    [HttpGet(CarePages.ChartSave)]
    public IActionResult OfAResident() => View("/Care/Admission/ChartSave.cshtml", new ChartSave { Diet = "Soft" });

    [HttpPost(CareEndpoints.ChartSaves)]
    public async Task<IActionResult> Save([FromForm] ChartSave chart)
    {
        await charts.SetStringAsync(SavesOnFile, (await SavesReceived() + 1).ToString(CultureInfo.InvariantCulture));
        await World.Reach(GateName.Of(LanesGate.Respond), Request.Path);
        return Content(await SavedLine(), PlainText);
    }

    private WorldData World => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;

    private async Task<int> SavesReceived() => await charts.GetStringAsync(SavesOnFile) is { } saves ? int.Parse(saves, CultureInfo.InvariantCulture) : 0;

    private async Task<string> SavedLine() => await SavesReceived() switch
    {
        1 => "Saved once",
        2 => "Saved twice",
        var saves => string.Create(CultureInfo.InvariantCulture, $"Saved {saves} times"),
    };
}

public sealed class ChartSave
{
    public string? Diet { get; set; }
}

public static class ChartSaveButtons
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
