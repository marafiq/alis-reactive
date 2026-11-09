using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Cards;

public sealed class CardsController : Controller
{
    public IActionResult Ward() => View(new CardsWard());

    public IActionResult Card(string resident) => PartialView(new CareCard(resident));

    public IActionResult Missing() => NotFound();

    public IActionResult Listed() => Json(new CareCard("Ada Byron"));

    public async Task<EmptyResult> Broken()
    {
        Response.ContentType = "text/html; charset=utf-8";
        Response.ContentLength = 1000;
        await Response.WriteAsync("<h2>Ben Okafor</h2><p>Room 4");
        await Response.Body.FlushAsync();
        return new EmptyResult();
    }

    public IActionResult RelativeUrl() => View(new CardsWard());

    public IActionResult FragmentUrl() => View(new CardsWard());

    public IActionResult PlaceholderUrl() => View(new CardsWard());

    public IActionResult UnpairedBraceUrl() => View(new CardsWard());

    public IActionResult IntoAfterAStep() => View(new CardsWard());

    public IActionResult IntoTwice() => View(new CardsWard());

    public IActionResult Radios() => View(new CardsWard());

    public IActionResult Admitted() => View(new CardsWard());

    public IActionResult Bedside() => View(new CardsWard());

    public IActionResult Handled() => View(new CardsWard());

    public IActionResult ListenWritesThePayload() => View(new CardsWard());

    public IActionResult ListenOnNothing() => View(new CardsWard());

    public IActionResult TextBeforeItsSlot() => View(new CardsWard());

    public IActionResult WardNotes() => PartialView();

    public IActionResult HtmlAfterItsUnload() => View(new CardsWard());

    public IActionResult Notes(string resident) => PartialView(new CareCard(resident, NotesTitle: "Latest notes"));
}

public sealed record CardsWard;

public sealed record CareCard(string Resident, bool NotesOpen = false, string NotesTitle = "Notes");
