using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class KinCardsController : Controller
{
    [HttpGet(CarePages.KinCards)]
    public IActionResult OfAResident() => View("/Care/Admission/KinCards.cshtml", new KinCards());
}

public sealed class KinCards
{
    public KinCardContact NextOfKin { get; set; } = new();

    public KinCardContact Alternate { get; set; } = new();
}

public sealed class KinCardContact
{
    public string? Name { get; set; }
}
