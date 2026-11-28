using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class VisitChoicesController : Controller
{
    [HttpGet(CarePages.VisitChoices)]
    public IActionResult OfTheWeek() => View("/Care/RecordContacts/VisitChoices.cshtml", new VisitChoices());
}

public sealed class VisitChoices;
