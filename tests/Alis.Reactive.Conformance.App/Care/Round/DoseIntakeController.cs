using System.Globalization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DoseIntakeController(IDistributedCache intakes) : Controller
{
    private const string PlainText = "text/plain";
    private const string SavesOnFile = "round-dose-intake-saves";

    [HttpGet(CarePages.DoseIntake)]
    public IActionResult OfARound() => View("/Care/Round/DoseIntake.cshtml", new DoseIntake("Ruth Ellis", "Paracetamol"));

    [HttpPost(CareEndpoints.DoseIntakeSaves)]
    public async Task<IActionResult> Save()
    {
        var saves = await SavesReceived() + 1;
        await intakes.SetStringAsync(SavesOnFile, saves.ToString(CultureInfo.InvariantCulture));
        return Content(string.Create(CultureInfo.InvariantCulture, $"saved {saves}"), PlainText);
    }

    private async Task<int> SavesReceived() => await intakes.GetStringAsync(SavesOnFile) is { } saves ? int.Parse(saves, CultureInfo.InvariantCulture) : 0;
}

public sealed record DoseIntake(string Resident, string Medicine);

public static class DoseIntakeEvents
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
