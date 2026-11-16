using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class MoodCheckController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.MoodCheck)]
    public IActionResult ForAResident() => View("/Care/Record/MoodCheck.cshtml", new MoodCheck("Ada Hart", false, null));

    [HttpPost(MoodCheckEndpoints.Checks)]
    public IActionResult Save([FromBody] MoodNote check) => Content(check.Mood ?? "cleared", PlainText);
}

public sealed record MoodCheck(string Resident, bool AskedAboutMood, string? Mood);

public sealed class MoodNote
{
    public string? Mood { get; set; } = "not asked";
}

public static class MoodCheckEndpoints
{
    public const string Checks = CarePages.MoodCheck + "/checks";
}

public static class MoodChecks
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
