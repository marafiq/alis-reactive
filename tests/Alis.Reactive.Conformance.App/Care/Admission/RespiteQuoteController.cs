using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RespiteQuoteController : Controller
{
    [HttpGet(CarePages.RespiteQuote)]
    public IActionResult OfAResident() => View("/Care/Admission/RespiteQuote.cshtml", new RespiteQuote());
}

public sealed class RespiteQuote
{
    public int Nights { get; set; } = 14;
}

public sealed class PreviousStayCard
{
    public int Nights { get; set; }
}

public static class RespiteQuotes
{
    public static readonly AppEvent Quote = AppEvent.Declare();
}
