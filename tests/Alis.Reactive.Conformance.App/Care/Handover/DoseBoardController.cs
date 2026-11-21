using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DoseBoardController : Controller
{
    [HttpGet(CarePages.DoseBoard)]
    public IActionResult OfTheRound()
        => View("/Care/Handover/DoseBoard.cshtml", new DoseBoard { Doses = [new DoseCard("Aspirin", "Due"), new DoseCard("Paracetamol", "Given")] });
}

public sealed class DoseBoard
{
    public IReadOnlyList<DoseCard> Doses { get; init; } = [];
}

public sealed record DoseCard(string Medicine, string Status);
