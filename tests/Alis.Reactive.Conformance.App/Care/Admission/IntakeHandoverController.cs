using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class IntakeHandoverController : Controller
{
    private const string PlainText = "text/plain";
    private const string Page = "/Care/Admission/IntakeHandover.cshtml";

    [HttpGet(CarePages.IntakeHandover)]
    public IActionResult Open() => View(Page, new IntakeHandover("", "Night", ""));

    [HttpPost(IntakeHandoverEndpoints.Handovers)]
    public IActionResult HandOver() => Content(Echo(Request.Form), PlainText);

    [HttpGet(IntakeHandoverEndpoints.Handovers)]
    public IActionResult LookUp() => Content(Echo(Request.Query), PlainText);

    private static string Echo(IEnumerable<KeyValuePair<string, StringValues>> fields)
        => string.Join("; ", fields.OrderBy(field => field.Key, StringComparer.Ordinal).Select(field => $"{field.Key} {field.Value}"));
}

public sealed record IntakeHandover(string Name, string Shift, string Desk);

public static class IntakeHandoverEndpoints
{
    public const string Handovers = CarePages.Area + "admission/intake-handover/handovers";
}

public static class IntakeHandovers
{
    public static readonly AppEvent HandOver = AppEvent.Declare();

    public static readonly AppEvent LookUp = AppEvent.Declare();
}
