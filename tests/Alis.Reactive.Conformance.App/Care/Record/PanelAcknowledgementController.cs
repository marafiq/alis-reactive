using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class PanelAcknowledgementController : Controller
{
    [HttpGet(CarePages.PanelAcknowledgement)]
    public IActionResult OfAResident() => View("/Care/Record/PanelAcknowledgement.cshtml", new PanelAcknowledgement("Ada Hart", "Night round", "Gamma"));
}

public sealed record PanelAcknowledgement(string Resident, string Round, string Outer);
