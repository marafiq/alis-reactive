using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DischargeAccountController : Controller
{
    [HttpGet(CarePages.DischargeAccount)]
    public IActionResult OfAResident() => View("/Care/Record/DischargeAccount.cshtml", new DischargeAccount("Ada Hart", 40));
}

public sealed record DischargeAccount(string Resident, int Balance);

public static class DischargeAccounts
{
    public static readonly AppEvent Discharge = AppEvent.Declare();
}
