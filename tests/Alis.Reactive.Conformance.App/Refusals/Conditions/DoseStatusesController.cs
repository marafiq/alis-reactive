using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class DoseStatusesController : Controller
{
    [HttpGet(RefusalPages.StatusesWrittenInTwoForms)]
    public IActionResult WrittenInTwoForms() => View("/Refusals/Conditions/DoseStatusesWrittenInTwoForms.cshtml", new StatusCheck("Aspirin"));

    [HttpGet(RefusalPages.StatusesWrittenByTheApp)]
    public IActionResult WrittenByTheApp() => View("/Refusals/Conditions/DoseStatusesWrittenByTheApp.cshtml", new StatusCheck("Aspirin"));

    [HttpGet(ConditionEndpoints.StatusesInTwoForms)]
    public IActionResult StatusesInTwoForms() => Json(new StatusesInTwoForms(
        [new("Aspirin", RoundStatus.Given), new("Digoxin", RoundStatus.Due)],
        [new("Aspirin", RoundStatus.Given), new("Digoxin", RoundStatus.Held)]));

    [HttpGet(ConditionEndpoints.StatusesInOneForm)]
    public IActionResult StatusesInOneForm() => Json(new StatusesInOneForm(
        [new("Aspirin", RoundStatus.Given), new("Digoxin", RoundStatus.Due)],
        [new("Aspirin", RoundStatus.Given), new("Digoxin", RoundStatus.Held)]));
}

public sealed record StatusCheck(string Medicine);

public sealed record StatusesInTwoForms(List<PlannedStatus> Planned, List<ChartedStatus> Charted);

public sealed record StatusesInOneForm(List<PlannedStatus> Planned, List<PlannedStatus> Charted);

public sealed record PlannedStatus(string Medicine, RoundStatus Status);

public sealed record ChartedStatus(string Medicine, [property: JsonConverter(typeof(JsonStringEnumConverter))] RoundStatus Status);

public enum RoundStatus
{
    Due,

    Given,

    Held,
}

public static class StatusChecks
{
    public static readonly AppEvent Compare = AppEvent.Declare();
}
