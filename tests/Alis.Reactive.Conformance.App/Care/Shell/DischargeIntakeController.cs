using System.Globalization;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DischargeIntakeController(IDistributedCache intakes) : Controller
{
    private const string PlainText = "text/plain";
    private const string SavesOnFile = "shell-discharge-intake-saves";

    [HttpGet(CarePages.DischargeIntake)]
    public IActionResult OfAResident() => View("/Care/Shell/DischargeIntake.cshtml", new DischargeIntake("Ada"));

    [HttpGet(CareEndpoints.DischargeIntakeAmendedName)]
    [AtTheAnswerGate]
    public IActionResult AmendedName() => Json(new IntakeSave { Name = "Ada Lovelace" });

    [HttpPost(DischargeIntakeEndpoints.Saves)]
    public async Task<IActionResult> Save([FromForm] IntakeSave intake)
    {
        var saves = await SavesReceived() + 1;
        await intakes.SetStringAsync(SavesOnFile, saves.ToString(CultureInfo.InvariantCulture));
        return Content(string.Create(CultureInfo.InvariantCulture, $"{saves} {intake.Name}"), PlainText);
    }

    private async Task<int> SavesReceived() => await intakes.GetStringAsync(SavesOnFile) is { } saves ? int.Parse(saves, CultureInfo.InvariantCulture) : 0;
}

public sealed record DischargeIntake(string Resident);

public sealed class IntakeSave
{
    public string? Name { get; set; }
}

public static class IntakeEvents
{
    public static readonly AppEvent<IntakeSave> IntakeSaveRequested = AppEvent.Declare<IntakeSave>();
}

public static class DischargeIntakeButtons
{
    public static readonly AppEvent Save = AppEvent.Declare();
}

public static class DischargeIntakeEndpoints
{
    public const string Saves = CarePages.Area + "shell/discharge-intake/saves";
}
