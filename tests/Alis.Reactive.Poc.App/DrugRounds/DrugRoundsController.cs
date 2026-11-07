using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.DrugRounds;

public sealed class DrugRoundsController : Controller
{
    public IActionResult Round() => View(new DrugRound());

    public IActionResult Trolley() => View(new DrugRound());

    public IActionResult NoteAfterAwait() => View(new DrugRound());

    public IActionResult SignatureAfterAwait() => View(new DrugRound());
}

public sealed record DrugRound;
