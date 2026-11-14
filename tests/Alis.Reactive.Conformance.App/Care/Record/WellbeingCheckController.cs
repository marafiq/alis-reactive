using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WellbeingCheckController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.WellbeingCheck)]
    public IActionResult ForAResident() => View("/Care/Record/WellbeingCheck.cshtml", new WellbeingCheck("Ada Hart", false, null));

    [HttpPost(WellbeingCheckEndpoints.Checks)]
    public IActionResult Save([FromForm] WellbeingNote check) => Content($"{check.Note}", PlainText);
}

public sealed record WellbeingCheck(string Resident, bool IncludeNote, string? Note);

public sealed class WellbeingNote
{
    public string? Note { get; set; } = "none";
}

public static class WellbeingCheckEndpoints
{
    public const string Checks = CarePages.WellbeingCheck + "/checks";
}

public static class WellbeingChecks
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
