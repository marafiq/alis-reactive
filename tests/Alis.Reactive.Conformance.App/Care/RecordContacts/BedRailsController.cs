using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BedRailsController : Controller
{
    private const string Rails = "/Care/RecordContacts/BedRails.cshtml";

    [HttpGet(CarePages.BedRailsUp)]
    public IActionResult Up() => View(Rails, new BedRails { BedRailsUp = true });

    [HttpGet(CarePages.BedRailsDown)]
    public IActionResult Down() => View(Rails, new BedRails { BedRailsUp = false });
}

public sealed class BedRails
{
    public bool BedRailsUp { get; set; }
}
