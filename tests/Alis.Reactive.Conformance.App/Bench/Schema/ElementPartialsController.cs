using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class ElementPartialsController : Controller
{
    [HttpGet(BenchPages.SchemaElementPartials)]
    public IActionResult ForRoomFour() => View("/Bench/Schema/ElementPartials.cshtml", new ElementPartialsBoard("4"));

    [HttpGet(BenchPages.SchemaElementPartialsForRoomFive)]
    public IActionResult ForRoomFive() => View("/Bench/Schema/ElementPartials.cshtml", new ElementPartialsBoard("5"));
}

public sealed record ElementPartialsBoard(string Room);

public sealed record KeypadPart(string Caption);

public sealed record SignalPart(string Caption);
