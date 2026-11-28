using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class IntakeRecordController : Controller
{
    [HttpGet(CarePages.IntakeRecord)]
    public IActionResult OfAResident() => View("/Care/Record/IntakeRecord.cshtml", new IntakeRecord("Ada Hart", 250));
}

public sealed record IntakeRecord(string Resident, int Intake);

public static class IntakeRecords
{
    public static readonly AppEvent RecordTheIntake = AppEvent.Declare();
}
