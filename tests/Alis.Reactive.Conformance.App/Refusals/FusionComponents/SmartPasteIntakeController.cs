using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class SmartPasteIntakeController : Controller
{
    [HttpGet(RefusalPages.SmartPasteWithNoEndpoint)]
    public IActionResult WithNoEndpoint() => View("/Refusals/FusionComponents/SmartPasteIntake.cshtml", new PasteIntake { AssistEndpoint = "" });

    [HttpGet(RefusalPages.SmartPasteWithAnEndpoint)]
    public IActionResult WithAnEndpoint() => View("/Refusals/FusionComponents/SmartPasteIntake.cshtml", new PasteIntake { AssistEndpoint = "/intake/smart-paste" });
}

public sealed class PasteIntake
{
    public string AssistEndpoint { get; init; } = null!;
}
