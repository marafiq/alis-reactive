using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Stations;

public sealed class StationsController : Controller
{
    public IActionResult Census() => View(new CensusPage());

    public IActionResult StepInsideAList() => View(new CensusPage());

    public IActionResult Questions() => View(new CensusPage());

    public IActionResult HandWrittenSource() => View(new CensusPage());
}

public sealed record CensusPage;
