using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Cards;

public sealed class CardsController : Controller
{
    public IActionResult Ward() => View(new CardsWard());

    public IActionResult Card(string resident) => PartialView(new CareCard(resident));

    public IActionResult Missing() => NotFound();

    public IActionResult Listed() => Json(new CareCard("Ada Byron"));

    public IActionResult RelativeUrl() => View(new CardsWard());

    public IActionResult IntoAfterAStep() => View(new CardsWard());

    public IActionResult IntoTwice() => View(new CardsWard());

    public IActionResult Radios() => View(new CardsWard());

    public IActionResult Admitted() => View(new CardsWard());

    public IActionResult Bedside() => View(new CardsWard());

    public IActionResult Notes(string resident) => PartialView(new CareCard(resident, NotesTitle: "Latest notes"));
}

public sealed record CardsWard;

public sealed record CareCard(string Resident, bool NotesOpen = false, string NotesTitle = "Notes");
