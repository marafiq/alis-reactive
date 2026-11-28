using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class ContactBoardController : Controller
{
    [HttpGet(RefusalPages.ContactBoardByAddedContacts)]
    public IActionResult ByAddedContacts() => View("/Refusals/FusionTemplates/ContactBoard.cshtml", new ContactBoard([new VisitedResident("Ada", 3, 2), new VisitedResident("Ruth", 1, 1)]));

    [HttpGet(RefusalPages.ContactBoardByMultipliedContacts)]
    public IActionResult ByMultipliedContacts() => View("/Refusals/FusionTemplates/ContactBoardByMultipliedContacts.cshtml", new ContactBoard([new VisitedResident("Ada", 3, 2), new VisitedResident("Ruth", 1, 1)]));
}

public sealed record ContactBoard(IReadOnlyList<VisitedResident> Residents);

public sealed record VisitedResident(string Name, int Visits, int Calls);
