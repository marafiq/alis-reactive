using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class LedgerController : Controller
{
    private const decimal TheLastWholeNumberJavaScriptHolds = 9007199254740991m;

    [HttpGet(RefusalPages.LedgerPastWholeNumbers)]
    public IActionResult PastWholeNumbers() => View("/Refusals/ValuesAndSources/Ledger.cshtml", new Ledger("Ada Hart", TheLastWholeNumberJavaScriptHolds + 2));

    [HttpGet(RefusalPages.LedgerWithinWholeNumbers)]
    public IActionResult WithinWholeNumbers() => View("/Refusals/ValuesAndSources/Ledger.cshtml", new Ledger("Ada Hart", TheLastWholeNumberJavaScriptHolds));
}

public sealed record Ledger(string Resident, decimal Balance);
