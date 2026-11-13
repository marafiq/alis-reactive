using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class MedicationAssistantController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.MedicationAssistant)]
    public IActionResult ByTheBook() => View("/Care/Handover/MedicationAssistant.cshtml", new MedicationAssistant("Morning round"));

    [HttpGet(CarePages.MedicationAssistantOnCall)]
    public IActionResult OnCall() => View("/Care/Handover/MedicationAssistantOnCall.cshtml", new MedicationAssistant("Morning round"));

    [HttpGet(MedicationAssistantEndpoints.OnCallAnswer)]
    public IActionResult OnCallAnswer() => Content("Check the chart, then call the pharmacist.", PlainText);
}

public sealed record MedicationAssistant(string Round);

public static class MedicationAssistantEndpoints
{
    public const string OnCallAnswer = CarePages.MedicationAssistantOnCall + "/answer";
}
